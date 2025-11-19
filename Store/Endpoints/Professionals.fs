module Store.Endpoints.Professionals

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain.Store
open Store.Domain.Store.Catalog
open Store.Domain.Store.Interfaces
open Store.Endpoints.Queries

module Update =
  let private workflow (stx: ServiceContext) (input: Professional) =
    let db = stx.context.Plug<IProfessionalRepository>()
    let service = ProfessionalService.update stx.userSession db.Update db.Find

    async { return! service input }

  let handler: HttpHandler = Request.mapJson<Professional> <| runAsync workflow
  let endpoint = put "/professionals/{id}" handler

module Create =
  let private workflow (stx: ServiceContext) (input: Professional) =
    let db = stx.context.Plug<IProfessionalRepository>()
    let service = ProfessionalService.create stx.userSession db.Add

    async { return! service input }

  let handler: HttpHandler = Request.mapJson<Professional> <| runAsync workflow
  let endpoint = post "/professionals" handler

module Details =
  let private workflow (stx: ServiceContext) () =
    let db = stx.context.Plug<ProfessionalQuery>()

    async { return! db.GetAllPrivate(stx.userSession) }

  let allEndpoint = get "professionals" (runAsync workflow ())

module Private =

  let private workflow (stx: ServiceContext) () =
    let db = stx.context.Plug<ProfessionalQuery>()

    async { return! db.GetAllPrivate(stx.userSession) }

  let allEndpoint = get "professionals" (runAsync workflow ())

module Public =
  let private workflow (stx: ServiceContext) () =
    let db = stx.context.Plug<ProfessionalQuery>()

    async { return! db.GetAllPublic(stx.userSession) }

  let allEndpoint = get "public/professionals" (runAnonAsync workflow ())

let endpoints =
  [ Public.allEndpoint; Private.allEndpoint; Create.endpoint; Update.endpoint ]
