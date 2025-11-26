module Store.Endpoints.Products

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain.Store
open Store.Domain.Store.Interfaces
open Store.Domain.Store.ProductService

module Update =
  let private workflow (stx: ServiceContext) (input: ProductServiceInput) =
    let db = stx.context.Plug<IProductRepository>()
    let dbProf = stx.context.Plug<IProfessionalRepository>()
    let dbPt = stx.context.Plug<IProductTypeRepository>()

    let service =
      ProductService.update stx.userSession db.Update db.Find dbPt.Find dbProf.Find

    async { return! service input }

  let handler: HttpHandler = Request.mapJson<ProductServiceInput> <| runAsync workflow
  let endpoint = put "/products/{id}" handler

module Create =
  let private workflow (stx: ServiceContext) (input: ProductServiceInput) =
    let db = stx.context.Plug<IProductRepository>()
    let dbPt = stx.context.Plug<IProductTypeRepository>()
    let dbProf = stx.context.Plug<IProfessionalRepository>()
    let service = ProductService.create stx.userSession db.Add dbPt.Find dbProf.Find

    async { return! service input }

  let handler: HttpHandler = Request.mapJson<ProductServiceInput> <| runAsync workflow
  let endpoint = post "/products" handler

module Details =

  let private workflow (stx: ServiceContext) (id: string) =
    let db = stx.context.Plug<IProductRepository>()

    async { return! db.Find(id) }

  let httpHandler: HttpHandler =
    Request.mapRoute (fun q -> q.GetString("id")) <| runAnonAsync workflow

  let endpoint = get "/products/{id}" httpHandler

let private workflow (stx: ServiceContext) (input) =
  let db = stx.context.Plug<IProductRepository>()

  async { return! db.Fetch(input) }

let httpHandler: HttpHandler =
  Request.mapQuery (fun q ->
    { professionalId = q.TryGetString("professionalId")
      productTypeId = q.TryGetString("productTypeId")
      search = q.TryGetString("search")
      limit = q.GetInt32("limit", 1000)
       })
  <| runAnonAsync workflow

let allEndpoint = get "/products" httpHandler

let endpoints = [ allEndpoint; Details.endpoint; Create.endpoint; Update.endpoint ]
