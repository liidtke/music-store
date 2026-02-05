module Store.Domain.Store.Ordering

open System

open Catalog
open Store.Domain.Store.Catalog

type Phone = {
  number: string
}

type Address = {
  street: string
  number: string
  complement: string
  zipCode: string
  city: string
  state: string
}

type Customer = {
  //same as userid
  id: Id
  name: string //same 
  phone: Phone
  address: Address option
  document:string //cpf or cnpj
}

type BasketItem = { product: Product; quantity: int }

type Basket =
  { id: Id
    items: BasketItem list
    occurred: DateTime }

type OrderItem =
  { id: Guid
    product: Product
    quantity: int
    price: decimal
     }

type OrderStatus =
  | Submitted = 1
  | Confirmed = 2
  | InProgress = 3
  | Fulfilled = 4
  | Cancelled = 5

type PaymentStatus =
  | Pending = 1
  | Paid = 2
  | Refunded = 3
  
type CustomerOrderData =
  { userId: string
    name: string
    email: string
    document: string
    phone: string }

type PaymentMethod =
  | Direct = 0
  | Pix = 1
  | Money = 2//maybe used in future
  | CreditCard = 3 //maybe used in future


type Order =
  { id: Id
    occurred: DateTime
    status: OrderStatus
    value: decimal
    items: OrderItem list
    // responsibleId: Id
    customer: CustomerOrderData
    message:string option
    deliveryDate: DateTime
    paymentStatus: PaymentStatus
    paymentMethod: PaymentMethod
  }

//v2
type Invoice =
  {
    id:Id
    orderId:Id
    beneficiary: Professional
    paymentMethod: PaymentMethod
    value: decimal
    fileId: Id option } //id for file metadata

//v2
type OrderHistory =
  { id: Id
    orderId: Id
    description: string
    status: OrderStatus }


//events
  
type OrderPlacedEvent = { order: Order }
