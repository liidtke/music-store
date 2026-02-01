module Store.Endpoints.Orders

open System.Threading.Tasks
open Application
open Falco
open Falco.Routing
open Store
open Store.Domain.Store
open Store.Domain.Store.Interfaces
open Store.Domain.Store.OrderService
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

  let updatePayment (stx: ServiceContext) =
    Update.updateOrderPayment stx.userSession <| orderEffects stx Mode.Update


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

let toStatus (input: string) : OrderStatus =
  match input with
  | "Submitted" -> OrderStatus.Submitted
  | "Confirmed" -> OrderStatus.Confirmed
  | "InProgress" -> OrderStatus.InProgress
  | "Fulfilled" -> OrderStatus.Fulfilled
  | "Cancelled" -> OrderStatus.Cancelled
  | _ -> failwith "Invalid order status"


module Create =
  open Utils
  
  type OrderInputModel = { paymentMethod: string }

  let private workflow (stx: ServiceContext) (input: OrderInputModel) =
    let effects = Factory.create stx
    let bus = stx.context.Plug<IMessageBus>()


    async {
      use! lock = NamedAsyncLock.AcquireAsync($"user:{stx.userSession.userId}") |> Async.AwaitTask
      // do! Task.Delay(2000) |> Async.AwaitTask
      let! result = effects { paymentMethod = toPaymentMethod input.paymentMethod }
      do Events.send bus result
      return result.result
    }

  let handler: HttpHandler = Request.mapJson<OrderInputModel> <| runAsync workflow
  let endpoint = post "/orders/" handler

module Search =

  let workflow (stx: ServiceContext) (input: OrderSearchCriteria) =
    let query = stx.context.Plug<OrderQuery>()

    async {
      let! result = query.GetAll(input, stx.userSession)
      return result
    }

  let handler: HttpHandler =
    Request.mapQuery (fun q ->
      { userId = q.TryGetString "userId"
        startDate = q.TryGetDateTime "startDate"
        status = q.TryGetString "status" |> maybeMap toStatus
        paymentStatus = q.TryGetString "paymentStatus" |> maybeMap toPaymentStatus })
    <| runAsync workflow

  let endpoint = get "/orders/" handler

let endpoints = [ Create.endpoint; Search.endpoint ]
