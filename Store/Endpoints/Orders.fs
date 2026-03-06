module Store.Endpoints.Orders

open Application
open Falco
open Falco.Routing
open Store
open Store.Domain.Store
open Store.Domain.Store.Interfaces
open Store.Domain.Store.OrderService
open Store.Domain.Store.OrderService.Update
open Store.Domain.Store.Ordering
open Store.Endpoints.Queries
open Wolverine

module Factory =
  type Mode =
    | Create
    | Update

  let orderEffects (stx: ServiceContext) mode : OrderEffects =
    let orderRepository = stx.context.Plug<IOrderRepository>()
    let customerRepository = stx.context.Plug<ICustomerRepository>()
    let basketRepository = stx.context.Plug<IBasketRepository>()

    { save =
        if mode = Mode.Create then
          orderRepository.Add
        else
          orderRepository.Update
      fetch = orderRepository.Find
      fetchCustomer = customerRepository.Find
      fetchBasket = basketRepository.Find
      deleteBasket = basketRepository.Delete }

  let create (stx: ServiceContext) =
    OrderService.createOrder stx.userSession <| orderEffects stx Mode.Create

  let updateStatus (stx: ServiceContext) =
    Update.updateOrderStatus stx.userSession <| orderEffects stx Mode.Update

  let updateForAdmin (stx: ServiceContext) =
    Update.updateAdminOrder stx.userSession <| orderEffects stx Mode.Update


let toPaymentMethod pm =
  match pm with
  | "Direct" -> PaymentMethod.Direct
  | "Pix" -> PaymentMethod.Pix
  | _ -> failwith "Invalid payment method"

let toPaymentStatus (input: string) : PaymentStatus =
  match input with
  | "Paid" -> PaymentStatus.Paid
  | "Refunded" -> PaymentStatus.Refunded
  | _ -> PaymentStatus.Pending

let toStatus (input: string) : OrderStatus option =
  match input with
  | "Submitted" -> OrderStatus.Submitted |> Some
  | "Confirmed" -> OrderStatus.Confirmed |> Some
  | "InProgress" -> OrderStatus.InProgress |> Some
  | "Fulfilled" -> OrderStatus.Fulfilled |> Some
  | "Cancelled" -> OrderStatus.Cancelled |> Some
  | _ -> None


module Create =
  open Utils

  type OrderInputModel =
    { paymentMethod: string
      message: string option }

  let private workflow (stx: ServiceContext) (input: OrderInputModel) =
    let effects = Factory.create stx
    let bus = stx.context.Plug<IMessageBus>()

    async {
      use! lock = NamedAsyncLock.AcquireAsync($"user:{stx.userSession.userId}") |> Async.AwaitTask
      // do! Task.Delay(2000) |> Async.AwaitTask
      let! result =
        effects
          { paymentMethod = toPaymentMethod input.paymentMethod
            message = input.message }

      do Events.send bus result
      return result.result
    }

  let handler: HttpHandler = Request.mapJson<OrderInputModel> <| runAsync workflow
  let endpoint = post "/orders/" handler


module Update =
  open Utils
  open System

  let private workflow (stx: ServiceContext) (input: OrderUpdateInput) =
    let effects = Factory.updateForAdmin stx
    let bus = stx.context.Plug<IMessageBus>()

    async {
      use! lock = NamedAsyncLock.AcquireAsync($"user:{stx.userSession.userId}") |> Async.AwaitTask
      let! result = effects input

      do Events.send bus result
      return result.result
    }

  let handler: HttpHandler = Request.mapJson<OrderUpdateInput> <| runAsync workflow
  let endpoint = put "/orders/{id}" handler


module Details =

  let workflow (stx: ServiceContext) (input: string) =
    let query = stx.context.Plug<OrderQuery>()

    async {
      let! result = query.Get(input, stx.userSession)
      return result
    }

  let handler: HttpHandler =
    Request.mapRoute (fun r -> r.GetString("id")) <| runAsync workflow

  let endpoint = get "/orders/{id}" handler

module Search =

  let workflow (stx: ServiceContext) (input: OrderSearchCriteria) =
    let query = stx.context.Plug<OrderQuery>()

    async {
      let! result = query.GetAll(input, stx.userSession)
      return result
    }

  let handler: HttpHandler =
    Request.mapQuery (fun q ->
      { userId = q.TryGetString "userId" |> fromString
        startDate = q.TryGetDateTime "startDate"
        order = q.TryGetString "order" |> fromString
        status = q.TryGetString "status" |> optionMap toStatus
        limit = q.GetInt32("limit", 50)
        paymentStatus = q.TryGetString "paymentStatus" |> maybeMap toPaymentStatus })
    <| runAsync workflow

  let endpoint = get "/orders/" handler

let endpoints =
  [ Create.endpoint; Search.endpoint; Details.endpoint; Update.endpoint ]
