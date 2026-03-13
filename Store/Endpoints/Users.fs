module Store.Endpoints.Users

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain
open Store.Domain.Health
open Store.Domain.Health.UserService.Create
open Store.Domain.Health.UserService.Effects
open Store.Domain.Interfaces
open Store.Endpoints.Models
open Store.Infrastructure
open Store.Security
open Store.Session

module Current =
  let workflow (stx: ServiceContext) () =
    // let db = stx.getDbContext () |> UserData.factory
    let session = stx.sessionManager.GetSession stx.userSession.userId

    async {
      match session with
      | Some value -> return value |> UserSession.map |> succeed
      | None -> return invalidate "Error getting session"
    }

  let endpoint = get "/users/current" (runAsync workflow ())

module Details =
  let workflow (stx: ServiceContext) (id: string) =
    let db = stx.context.Plug<IUserRepository>()

    async {
      let! result = db.Find id
      return result |> perhaps UserModel.map
    }

  let handler: HttpHandler =
    runAsync workflow |> Request.mapRoute (fun q -> q.GetString("id"))

  let endpoint = get "/users/{id}" handler


module Update =
  type UserInput =
    { id: string
      name: string
      email: string
      subscription: string
      customRoleId: int option
      role: string
      isActive: bool }

  let updateWorkflow (stx: ServiceContext) (input: UserInput) =
    let db = stx.context.Plug<IUserRepository>()

    let user: User =
      { id = input.id
        email = input.email
        name = input.name
        password = None
        role = Models.Role.convert input.role
        isActive = input.isActive
        isVerified = false }

    async {
      let! result = UserService.Update.handle stx.userSession db user
      return result ?> (fun item -> Models.UserModel.map item |> succeed)
    }

  let handler input : HttpHandler =
    runAsync updateWorkflow |> Request.mapJson<UserInput>

  let updateRouteHandler: HttpHandler =
    Request.mapRoute (fun i ->
      let a = i.GetString("id", "")
      a)
    <| handler

  let endpoint = put "/users/{id}" updateRouteHandler

module Create =

  let createWorkflow (stx: ServiceContext) user =
    let db = stx.context.Plug<IUserRepository>()
    let secSet = stx.securitySettings
    let encryptEffect = hashPassword secSet.Iterations

    async {
      let! result = UserService.Create.handle stx.userSession encryptEffect db user
      return result |> perhaps UserModel.map
    }

  let handler: HttpHandler =
    runAnonAsync createWorkflow |> Request.mapJson<CreateUser>

  let endpoint = post "/users" handler

type private ListInput = { search: string }

let private workflow (stx: ServiceContext) (listInput: UserFetchParams) =
  let db = stx.context.Plug<IUserRepository>()
  let searchUserEffect = db.Fetch

  async {
    let! searchOutput = searchUserEffect listInput
    return searchOutput ?> (fun users -> List.map UserModel.map users |> succeed)
  }

open Models

let private getRole (name: string option) =
  match name with
  | Some x -> x |> Role.convert |> Some
  | None -> None

let handler: HttpHandler =
  runAsync workflow
  |> Request.mapQuery (fun q ->
    { name = q.GetString("name", null)
      email = q.GetString("email", null)
      role = q.TryGetString("role") |> getRole })


let usersEndpoint = get "/users" handler

let endpoints =
  [ usersEndpoint
    Create.endpoint
    Update.endpoint
    Details.endpoint
    Current.endpoint ]
