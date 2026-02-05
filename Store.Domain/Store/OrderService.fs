module Store.Domain.Store.OrderService


open System
open Store.Domain
open Store.Domain.Store.Catalog
open Store.Domain.Store.Ordering


type SaveEffect = Effect<Order>
type FetchEffect = FetchSingleEffect<string, Order>
type FetchCustomerEffect = FetchSingleEffect<string, Customer>
type FetchBasketEffect = FetchSingleEffect<string, Basket>
type DeleteBasketEffect = string -> Async<ServiceOutput<unit>>

type CreateOrderInput = { paymentMethod: PaymentMethod; message:string option }

type OrderEffects =
  { save: SaveEffect
    fetch: FetchEffect
    fetchCustomer: FetchCustomerEffect
    fetchBasket: FetchBasketEffect
    deleteBasket: DeleteBasketEffect }


let private convertItems (items: BasketItem list) : OrderItem list =
  items
  |> List.map (fun i ->
    { id = Guid.NewGuid()
      product = i.product
      quantity = i.quantity
      price = i.product.unitPrice * decimal i.quantity })

let private validateOrder (order: Order) =
  if order.value <= 0m then
    invalidate "Invalid Value"
  else
    succeed order

let calculateDeliveryDate (basket: Basket) : DateTime =
  let effort = basket.items |> List.sumBy (fun i -> i.product.effort * i.quantity)
  let deadLine = basket.items |> List.map (fun i -> i.product.minDeadLine) |> List.max

  let days = if effort < deadLine then deadLine else effort
  DateTime.UtcNow.AddDays(days)


let createOrder (us: UserSession) (effects: OrderEffects) (input: CreateOrderInput) =
  let basicFetch =
    async {
      let! basket = effects.fetchBasket us.userId
      let! customer = effects.fetchCustomer us.userId

      return
        match basket, customer with
        | Success basket, Success customer -> (basket, customer) |> succeed
        | Success basket, Failure errorResult -> Failure errorResult
        | Failure errorResult, Success customer -> Failure errorResult
        | Failure errorResult, Failure result -> Failure errorResult
    }

  let createOrder (basket: Basket, customer: Customer) =
    { id = ""
      occurred = DateTime.UtcNow
      items = convertItems basket.items
      deliveryDate = calculateDeliveryDate basket
      paymentStatus = PaymentStatus.Pending
      status = OrderStatus.Submitted
      paymentMethod = input.paymentMethod
      message = input.message
      value = basket.items |> List.sumBy (fun i -> i.product.unitPrice * decimal i.quantity)
      customer =
        { userId = customer.id
          name = customer.name
          document = customer.document
          email = us.email
          phone = customer.phone.number } }
    |> validateOrder

  async {
    let! result = basicFetch
    let order = result ?> createOrder
    let! saveResult = order ?>< effects.save
    let! deleteResult = saveResult ?>< (fun _ -> effects.deleteBasket us.userId)

    match saveResult with
    | Success order -> return DomainOutput.from saveResult <| Some { order = order }
    | Failure errorResult -> return DomainOutput.from saveResult None
  }


module Update =

  let validateStatus (status: OrderStatus) (order: Order) =
    if order.status = OrderStatus.Cancelled || order.status = OrderStatus.Fulfilled then
      invalidate "Order already processed"
    elif status = OrderStatus.Cancelled && order.paymentStatus <> PaymentStatus.Paid then
      invalidate "Order paid"
    else
      succeed order

  let validatePermission us (order: Order) =
    if us.role = AdminRole || us.role = ProfessionalRole then
      succeed order
    else
      invalidate "Permission denied"

  let updateOrderStatus (us: UserSession) (effects: OrderEffects) (orderId: string) (status: OrderStatus) =
    async {
      let! result = effects.fetch orderId

      let order =
        result
        ?> validatePermission us
        ?> validateStatus status
        ?> (fun order -> { order with status = status } |> succeed)

      let! saveResult = order ?>< effects.save

      return saveResult
    }

  let updateOrderPayment (us: UserSession) (effects: OrderEffects) (orderId: string) (status: PaymentStatus) =
    async {
      let! result = effects.fetch orderId

      let order =
        result
        ?> validatePermission us
        ?> (fun order -> { order with paymentStatus = status } |> succeed)

      let! saveResult = order ?>< effects.save

      return saveResult
    }
