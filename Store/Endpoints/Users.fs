module Store.Endpoints.Users

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain.Health
open Store.Domain.Health.Access
open Store.Domain.Health.UserService.Create
open Store.Domain.Health.UserService.Effects
open Store.Endpoints.Models
open Store.Infrastructure
open Store.Security
open Store.Session

(*
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
  let workflow (stx: ServiceContext) (id: int) =
    let db = stx.getDbContext () |> UserData.factory

    async {
      let! result = db.get id
      return result |> perhaps UserModel.map
    }

  let handler: HttpHandler =
    runAsync workflow |> Request.mapRoute (fun q -> q.GetInt32("id"))

  let endpoint = get "/users/{id}" handler


module Update =
  type UserInput =
    { id: int
      name: string
      email: string
      subscription: string
      customRoleId: int option
      role: string
      isDeleted: bool }

  let updateWorkflow (stx: ServiceContext) (input: UserInput) =
    let db = stx.getDbContext () |> UserData.factory

    let user: User =
      { id = input.id
        email = input.email
        name = input.name
        password = None
        role = Models.Role.convert input.role input.customRoleId
        isDeleted = input.isDeleted
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
    let db = stx.getDbContext () |> UserData.factory
    let secSet = stx.securitySettings
    let encryptEffect = hashPassword secSet

    let deps: Dependencies =
      { count = db.count
        save = db.save
        get = db.get }

    async {
      let! result = UserService.Create.handle stx.userSession encryptEffect deps user
      return result |> perhaps UserModel.map
    }

  let handler: HttpHandler =
    runAnonAsync createWorkflow |> Request.mapJson<CreateUser>

  let endpoint = post "/users" handler

type private ListInput = { search: string }

let private workflow (stx: ServiceContext) (listInput: ListInput) =
  let searchUserEffect = stx.getDbContext () |> UserData.searchUser

  async {
    let! searchOutput = searchUserEffect listInput.search
    return searchOutput ?> (fun users -> List.map UserModel.map users |> succeed)
  }

let handler: HttpHandler =
  runAsync workflow
  |> Request.mapQuery (fun q -> { search = q.GetString("search", null) })


let usersEndpoint = get "/users" handler

let endpoints =
  [ usersEndpoint
    Create.endpoint
    Update.endpoint
    Details.endpoint
    Current.endpoint ]
*)
