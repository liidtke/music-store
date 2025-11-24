module Store.Endpoints.ProductTypes

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain.Store
open Store.Domain.Store.Catalog
open Store.Domain.Store.Interfaces

module Update =
  let private workflow (stx: ServiceContext) (input: ProductType) =
    let db = stx.context.Plug<IProductTypeRepository>()
    let service = ProductTypeService.update stx.userSession db.Update

    async { return! service input }

  let handler: HttpHandler = Request.mapJson<ProductType> <| runAsync workflow
  let endpoint = put "/product-types/{id}" handler

module Create =
  let private workflow (stx: ServiceContext) (input: ProductType) =
    let db = stx.context.Plug<IProductTypeRepository>()
    let service = ProductTypeService.create stx.userSession db.Add

    async { return! service input }

  let handler: HttpHandler = Request.mapJson<ProductType> <| runAsync workflow
  let endpoint = post "/product-types" handler

module Details =

  let private workflow (stx: ServiceContext) (id: string) =
    let db = stx.context.Plug<IProductTypeRepository>()
    db.Find(id)

  let handler: HttpHandler =
    Request.mapRoute (fun r -> r.GetString("id")) <| runAsync workflow

  let endpoint = get "/product-types/{id}" handler


let private workflow (stx: ServiceContext) () =
  let db = stx.context.Plug<IProductTypeRepository>()

  async { return! db.All() }

let allEndpoint = get "/product-types" (runAnonAsync workflow ())

let endpoints = [ allEndpoint; Details.endpoint; Create.endpoint; Update.endpoint ]
