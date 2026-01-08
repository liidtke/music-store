module Store.Domain.Store.Entities

open System

open Catalog

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
  isBillingAddress: bool
}

type Customer = {
  //same as userid
  id: Id
  name: string //same 
  phones: Phone list
  addresses: Address list
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
  | AwaitingValidation = 2
  | Confirmed = 3
  | Paid = 4
  | InProgress = 5
  | Fulfilled = 7
  | Cancelled = 9

type UserOrderData =
  { userId: int
    name: string
    document: string
    address: string
    zipCode: string
    phone: string }

type PaymentMethod =
  | Pix
  | Money //maybe used in future
  | CreditCard //maybe used in future

type Invoice =
  { beneficiary: Professional
    paymentMethod: PaymentMethod
    value: decimal
    fileId: Id option } //id for file metadata

type Order =
  { id: Id
    occurred: DateTime
    status: OrderStatus
    bulkDiscount: int //ex 10%
    totalValue: decimal
    items: OrderItem
    dueDate: DateTime
    user: UserOrderData
    invoices: Invoice list
    receiptId: Id option  //receipt attached by user
  }

type OrderHistory =
  { id: Id
    orderId: Id
    description: string
    status: OrderStatus }
