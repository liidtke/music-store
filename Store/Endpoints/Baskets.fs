module Store.Endpoints.Baskets

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain
open Store.Domain.Store
open Store.Domain.Store.BasketService
open Store.Domain.Store.Interfaces


module Update =
  let private workflow (stx: ServiceContext) (input: BasketInput) =
    let db = stx.context.Plug<IBasketRepository>()
    let productRepo = stx.context.Plug<IProductRepository>()

    let service =
      BasketService.createOrUpdate stx.userSession db.Add db.Find productRepo.Find

    async { return! service input }

  let handler: HttpHandler = Request.mapJson<BasketInput> <| runAsync workflow

  let endpoint = put "/my-basket/" handler

module Mine =
  let private workflow (stx: ServiceContext) () =
    let db = stx.context.Plug<IBasketRepository>()

    async { return! db.Find(stx.userSession.userId) }

  // let handler: HttpHandler =
  //   Request.mapRoute (fun r -> r.GetString("id")) <| runAsync workflow

  let endpoint = get "/my-basket/" (runAnonAsync workflow ())

let endpoints = [ Update.endpoint; Mine.endpoint ]
