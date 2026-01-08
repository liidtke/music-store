module Store.Endpoints.Baskets

open System
open Application
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Mvc.ModelBinding.Binders
open Store
open Store.Domain
open Store.Domain.Store
open Store.Domain.Store.BasketService
open Store.Domain.Store.Entities
open Store.Domain.Store.Interfaces

type BasketModel =
  { id: Id
    items: BasketItem list
    occurred: DateTime
    price: decimal
    quantity: decimal }


let toModel (basket: Basket) : BasketModel =
  { id = basket.id
    items = basket.items
    occurred = basket.occurred
    price = basket.items |> List.sumBy (fun i -> i.product.unitPrice * decimal i.quantity)
    quantity = basket.items |> List.sumBy (fun i -> decimal i.quantity) }

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

    async {
      let! output = service <| mapBasketInputModel input
      return output ?> (fun i -> toModel i |> succeed)
    }

  let handler: HttpHandler = Request.mapJson<BasketInputModel> <| runAsync workflow

  let endpoint = put "/my-basket/" handler

module Mine =
  let private workflow (stx: ServiceContext) () =
    let db = stx.context.Plug<IBasketRepository>()

    async {
      let! b = db.Find(stx.userSession.userId)
      return b ?> (fun i -> toModel i |> succeed)
    }

  // let handler: HttpHandler =
  //   Request.mapRoute (fun r -> r.GetString("id")) <| runAsync workflow

  let endpoint = get "/my-basket/" (runAsync workflow ())

let endpoints = [ Update.endpoint; Mine.endpoint ]
