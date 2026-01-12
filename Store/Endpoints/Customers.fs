module Store.Endpoints.Customers

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain
open Store.Domain.Interfaces
open Store.Domain.Store
open Store.Domain.Store.Interfaces

module Create =
  let private workflow (stx: ServiceContext) (input: CustomerService.CustomerServiceInput) =
    let db = stx.context.Plug<ICustomerRepository>()
    let userRepo = stx.context.Plug<IUserRepository>()
    let service = CustomerService.create stx.userSession db.Add userRepo.Find

    async { return! service input }

  let handler: HttpHandler =
    Request.mapJson<CustomerService.CustomerServiceInput> <| runAsync workflow

  let endpoint = post "/customers" handler

module Update =
  let private workflow (stx: ServiceContext) (input: CustomerService.CustomerServiceInput) =
    let db = stx.context.Plug<ICustomerRepository>()
    let userRepo = stx.context.Plug<IUserRepository>()
    let service = CustomerService.update stx.userSession db.Update db.Find userRepo.Find

    async { return! service input }

  let handler: HttpHandler =
    Request.mapJson<CustomerService.CustomerServiceInput> <| runAsync workflow

  let endpoint = put "/customers/{id}" handler

module Details =
  let private workflow (stx: ServiceContext) (id: string) =
    let db = stx.context.Plug<ICustomerRepository>()

    async {
      if
        stx.userSession.role = Role.AdminRole
        || stx.userSession.role.IsProfessionalRole
        || stx.userSession.userId.ToString() = id
      then
        return! db.Find(id)
      else
        return invalidate "Access Denied"
    }

  let handler: HttpHandler =
    Request.mapRoute (fun r -> r.GetString("id")) <| runAsync workflow

  let endpoint = get "/customers/{id}" handler

let endpoints = [ Create.endpoint; Update.endpoint; Details.endpoint ]
