module Store.Utils


open System
open System.Collections.Concurrent
open System.Threading
open System.Threading.Tasks

type NamedAsyncLock() =

  // Main storage: string → SemaphoreSlim(1,1)
  static let locks = ConcurrentDictionary<string, SemaphoreSlim>()

  // We keep a very weak reference to the semaphores so they can be collected
  // when no longer needed (important for long-running services)
  static let cleanup =
    let timer =
      new Timer(
        callback =
          (fun _ ->
            locks
            |> Seq.filter (fun kv -> kv.Value.CurrentCount = 1 && not ((kv.Value :> obj).Equals(null)))
            |> Seq.iter (fun kv ->
              match locks.TryRemove(kv.Key) with
              | true, _ -> kv.Value.Dispose()
              | _ -> ())),
        state = null,
        dueTime = Timeout.Infinite,
        period = 30_000 
      )

    timer.Change(Timeout.Infinite, 30_000) |> ignore
    timer

  /// Acquire the lock for this key (async)
  static member AcquireAsync(key: string) : Task<IDisposable> =
    task {
      let semaphore = locks.GetOrAdd(key, fun _ -> new SemaphoreSlim(1, 1))

      do! semaphore.WaitAsync()

      return
        { new IDisposable with
            member _.Dispose() = semaphore.Release() |> ignore
        // We don't remove it immediately — cleanup timer will handle it
        }
    }

/// Usage examples
module Examples =

  let doImportantWorkForUser (userId: string) =
    async {
      use! lock = NamedAsyncLock.AcquireAsync($"user:{userId}") |> Async.AwaitTask

      // Only one operation per userId can run at the same time
      printfn "Starting work for %s" userId
      do! Async.Sleep 1500
      printfn "Finished work for %s" userId
    }


  let processOrder (orderId: string) =
    task {
      use! lock = NamedAsyncLock.AcquireAsync($"order:{orderId}")

      printfn "Processing order %s" orderId
      do! Task.Delay 1200
      printfn "Done with order %s" orderId
    }

  // Multiple orders can be processed in parallel, but same order only once
  let exampleManyOrders () =
    task {
      let! _ = [ 1..20 ] |> List.map (fun i -> processOrder $"ORD-{i % 5:D6}") |> Task.WhenAll
      ()
    }
