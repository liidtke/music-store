module Store.Domain.Interfaces

open System.Linq

type IRepository<'T> =
  abstract member Find: string -> Async<ServiceOutput<'T>>
  abstract member Add: 'T -> Async<ServiceOutput<'T>>
  abstract member Update: 'T -> Async<ServiceOutput<'T>>
  // abstract member Query: unit -> IQueryable<'T>
  // abstract member Delete: string -> Async<ServiceOutput<unit>>


type UserFetchParams = {
  name:string
  email:string
}

type IUserRepository =
  inherit IRepository<User>
  abstract member FetchByEmail: string -> Async<ServiceOutput<User>>
  abstract member Fetch: UserFetchParams -> Async<ServiceOutput<User list>>
  abstract member Count: string -> Async<ServiceOutput<int>>
