module Store.Endpoints.Baskets

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain
open Store.Domain.Store
open Store.Domain.Store.BasketService
open Store.Domain.Store.Interfaces


let convertBasketActionType (str: string) : BasketActionType =
  match str with
  | "AddItem" -> AddItem
  | "RemoveItem" -> RemoveItem
  | "RemoveAll" -> RemoveAll
  | _ -> failwith "todo"

type BasketInputModel =
  { productId: string
    quantity: int
    action: string }

let mapBasketInputModel (model: BasketInputModel) : BasketInput =
  { productId = model.productId
    quantity = model.quantity
    action = convertBasketActionType model.action }

module Update =
  let private workflow (stx: ServiceContext) (input: BasketInputModel) =
    let db = stx.context.Plug<IBasketRepository>()
    let productRepo = stx.context.Plug<IProductRepository>()

    let service =
      BasketService.createOrUpdate stx.userSession db.Add db.Find productRepo.Find

    async { return! service <| mapBasketInputModel input }

  let handler: HttpHandler =
    Request.mapJson<BasketInputModel> <| runAnonAsync workflow

  let endpoint = put "/my-basket/" handler

module Mine =
  let private workflow (stx: ServiceContext) () =
    let db = stx.context.Plug<IBasketRepository>()

    async { return! db.Find(stx.userSession.userId) }

  // let handler: HttpHandler =
  //   Request.mapRoute (fun r -> r.GetString("id")) <| runAsync workflow

  let endpoint = get "/my-basket/" (runAnonAsync workflow ())

let endpoints = [ Update.endpoint; Mine.endpoint ]
