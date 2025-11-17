module Store.Events

open Store.Domain.Health
open Store.Domain.Health.UserService
open Microsoft.Extensions.Logging
open Wolverine

type SuccessEvent<'T, 'TEvent> = { obj: 'T; domainEvent: 'TEvent }

let succeedWithEvent (createFunc: 'T -> 'TEvent) (func: ServiceOutput<'T>) : SuccessEvent<'T, 'TEvent> ServiceOutput =
  func ?> (fun i -> { obj = i; domainEvent = createFunc i } |> Success)

let send (bus: IMessageBus) (successEvent: SuccessEvent<'T, 'TEvent> ServiceOutput) =
  let send e =
    async {
      do! bus.PublishAsync(e.domainEvent).AsTask() |> Async.AwaitTask
      return ()
    }

  match successEvent with
  | Success successEvent -> send successEvent
  | Failure errorResult -> async { return () }

//events and handlers

