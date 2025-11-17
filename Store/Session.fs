module Store.Session

open Store.Domain.Health
open Store.Domain.Health.UserService
open Microsoft.Extensions.Caching.Memory
open System.Collections.Concurrent

type SessionManager(cache: IMemoryCache) =
  let cacheKey (userId: int) = $"session:{userId}"

  let getSession (userId: int) =
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

  let removeSession (userId: int) =
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

  member this.RemoveSession(userId: int) = removeSession userId

  member this.GetSession(userId: int) =
    let result = getSession userId
    result


let update (repository: IUserRepository) (sessionManager: SessionManager) (users: int list) =
  async {
    users |> List.map sessionManager.RemoveSession |> ignore
  // for user in users do
  //   let! result = repository.GetUser user
  //
  //   match result with
  //   | Success u -> sessionManager.RemoveSession(u.id) |> ignore
  //   | Failure err -> ()
  }
