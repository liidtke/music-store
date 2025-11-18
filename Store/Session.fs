module Store.Session

open Store.Domain
open Store.Domain.Health
open Store.Domain.Health.UserService
open Microsoft.Extensions.Caching.Memory
open System.Collections.Concurrent
open Store.Domain.Interfaces

type SessionManager(cache: IMemoryCache) =
  let cacheKey (userId: string) = $"session:{userId}"

  let getSession (userId: string) =
    try
      let result = userId |> cacheKey |> cache.Get<UserSession>

      match box result with
      | null -> None
      | o -> Some result
    with _ ->
      None

  let insertSession (session: UserSession) =
    let key = session.userId |> cacheKey
    cache.Set<UserSession>(key, session) |> ignore

  let removeSession (userId: string) =
    let key = userId |> cacheKey
    cache.Remove(key)

  member this.UpdateSession(user: User) =
    let newSession: UserSession =
      { userId = user.id
        email = user.email
        name = user.name
        role = user.role }

    insertSession newSession |> ignore
    newSession

  member this.AddSession(user: User) =
    let newSession: UserSession =
      { userId = user.id
        email = user.email
        name = user.name
        role = user.role }

    insertSession newSession |> ignore
    newSession

  member this.RemoveSession(userId: string) = removeSession userId

  member this.GetSession(userId: string) =
    let result = getSession userId
    result


let update (repository: IUserRepository) (sessionManager: SessionManager) (users: string list) =
  async {
    users |> List.map sessionManager.RemoveSession |> ignore
  // for user in users do
  //   let! result = repository.GetUser user
  //
  //   match result with
  //   | Success u -> sessionManager.RemoveSession(u.id) |> ignore
  //   | Failure err -> ()
  }
