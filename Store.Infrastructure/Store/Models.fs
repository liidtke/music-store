module Store.Infrastructure.Store.Models


open System
open MongoDB.Bson
open Store.Domain.Store.Catalog
open Store.Domain.Store.Ordering

[<CLIMutable>]
type ProductTypeData =
  { id: BsonObjectId
    name: string
    description: string option
    imageUrl: string
    isActive: bool }

module ProductTypeMapping =
  open System

  let toProductTypeData (productType: ProductType) : ProductTypeData =
    let id =
      if String.IsNullOrEmpty(productType.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(productType.id)

    { id = id
      name = productType.name
      description = productType.description
      imageUrl = productType.imageUrl
      isActive = productType.isActive }

  let toProductType (data: ProductTypeData) : ProductType =
    { id = data.id.ToString()
      name = data.name
      description = data.description
      imageUrl = data.imageUrl
      isActive = data.isActive }


[<CLIMutable>]
type ProfessionalData =
  { id: BsonObjectId
    name: string
    title: string
    document: string
    bulkDiscount: decimal
    isActive: bool
    pixKey: string option
    isManager: bool
    imageUrl: string option
    profile: string }

// === Mapping Module ===
module ProfessionalMapping =

  let toProfessionalData (professional: Professional) : ProfessionalData =
    let bsonId =
      if String.IsNullOrEmpty(professional.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(professional.id)

    { id = bsonId
      name = professional.name
      title = professional.title
      document = professional.document
      bulkDiscount = professional.bulkDiscount
      isActive = professional.isActive
      pixKey = professional.pixKey
      isManager = professional.isManager
      imageUrl = professional.imageUrl
      profile = professional.profile }

  let toProfessional (data: ProfessionalData) : Professional =
    { id = data.id.ToString()
      title = data.title
      name = data.name
      document = data.document
      bulkDiscount = data.bulkDiscount
      isActive = data.isActive
      pixKey = data.pixKey
      isManager = data.isManager
      imageUrl = data.imageUrl
      profile = data.profile }

[<CLIMutable>]
type ProductData =
  { id: BsonObjectId
    name: string
    professionalId: BsonObjectId
    professionalName: string
    productType: ProductTypeData
    description: string option
    unitPrice: decimal
    imageUrl: string option
    oldUnitPrice: decimal
    effort: int
    minDeadLine: int
    isActive: bool }

module ProductMapping =

  let toProductData (product: Product) : ProductData =
    let id =
      if String.IsNullOrEmpty(product.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(product.id)

    let professionalId =
      if String.IsNullOrEmpty(product.professionalId) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(product.professionalId)

    { id = id
      name = product.name
      professionalId = professionalId
      productType = ProductTypeMapping.toProductTypeData product.productType
      professionalName = product.professionalName
      description = product.description
      imageUrl = product.imageUrl
      unitPrice = product.unitPrice
      oldUnitPrice = product.oldUnitPrice
      effort = product.effort
      minDeadLine = product.minDeadLine
      isActive = product.isActive }

  let toProduct (data: ProductData) : Product =
    { id = data.id.ToString()
      name = data.name
      professionalId = data.professionalId.ToString()
      productType = ProductTypeMapping.toProductType data.productType
      imageUrl = data.imageUrl
      description = data.description
      professionalName = data.professionalName
      unitPrice = data.unitPrice
      oldUnitPrice = data.oldUnitPrice
      effort = data.effort
      minDeadLine = data.minDeadLine
      isActive = data.isActive }

[<CLIMutable>]
type SampleData =
  { id: BsonObjectId
    sampleType: int
    description: string
    professionalId: BsonObjectId
    productId: BsonObjectId }

[<CLIMutable>]
type SampleFileData =
  { id: BsonObjectId
    fileName: string
    mimeType: string
    contents: byte array }

module SampleMapping =

  let toSampleData (sample: Sample) : SampleData =
    let id =
      if String.IsNullOrEmpty(sample.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(sample.id)

    let productId =
      if String.IsNullOrEmpty(sample.productId) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(sample.productId)

    { id = id
      sampleType = int sample.sampleType
      professionalId = BsonObjectId.Create(sample.professionalId)
      description = sample.description
      productId = productId }

  let toSample (data: SampleData) : Sample =
    { id = data.id.ToString()
      sampleType = enum<SampleType> data.sampleType
      description = data.description
      professionalId = data.professionalId.ToString()
      productId = data.productId.ToString() }

  let toSampleFileData (file: SampleFile) : SampleFileData =
    let id =
      if String.IsNullOrEmpty(file.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(file.id)

    { id = id
      fileName = file.fileName
      mimeType = file.mimeType
      contents = file.contents }

  let toSampleFile (data: SampleFileData) : SampleFile =
    { id = data.id.ToString()
      fileName = data.fileName
      mimeType = data.mimeType
      contents = data.contents }


[<CLIMutable>]
type PhoneData = { number: string }

[<CLIMutable>]
type AddressData =
  { street: string
    number: string
    complement: string
    zipCode: string
    city: string
    state: string }

[<CLIMutable>]
type CustomerData =
  { id: BsonObjectId
    name: string
    phone: PhoneData
    address: AddressData option
    document: string }

module CustomerMapping =
  open Store.Domain.Store

  let toPhoneData (phone: Ordering.Phone) : PhoneData = { number = phone.number }

  let toPhone (data: PhoneData) : Ordering.Phone = { number = data.number }

  let toAddressData (addressOpt: Ordering.Address option) : AddressData option =
    match addressOpt with
    | None -> None
    | Some address ->
      { street = address.street
        number = address.number
        complement = address.complement
        zipCode = address.zipCode
        city = address.city
        state = address.state }
      |> Some

  let toAddress (ad: AddressData option) : Ordering.Address option =
    match ad with
    | None -> None
    | Some data ->
      ({ street = data.street
         number = data.number
         complement = data.complement
         zipCode = data.zipCode
         city = data.city
         state = data.state }
      : Ordering.Address)
      |> Some

  let toCustomerData (customer: Ordering.Customer) : CustomerData =
    let id =
      if String.IsNullOrEmpty(customer.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(customer.id)

    { id = id
      name = customer.name
      phone = customer.phone |> toPhoneData
      address = customer.address |> toAddressData
      document = customer.document }

  let toCustomer (data: CustomerData) : Ordering.Customer =
    { id = data.id.ToString()
      name = data.name
      phone = data.phone |> toPhone
      address = data.address |> toAddress
      document = data.document }


type BasketItemData = { product: ProductData; quantity: int }

[<CLIMutable>]
type BasketData =
  { id: BsonObjectId
    items: BasketItemData list
    occurred: DateTime }

module BasketMapping =

  let toBasketItemData (item: BasketItem) : BasketItemData =
    { product = ProductMapping.toProductData item.product
      quantity = item.quantity }

  let toBasketItem (data: BasketItemData) : BasketItem =
    { product = ProductMapping.toProduct data.product
      quantity = data.quantity }

  let toBasketData (basket: Basket) : BasketData =
    let id =
      if String.IsNullOrEmpty(basket.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(basket.id)

    { id = id
      items = basket.items |> List.map toBasketItemData
      occurred = basket.occurred }

  let toBasket (data: BasketData) : Basket =
    { id = data.id.ToString()
      items = data.items |> List.map toBasketItem
      occurred = data.occurred }


type OrderData =
  { id: BsonObjectId
    occurred: DateTime
    status: OrderStatus
    value: decimal
    items: OrderItem list
    customer: CustomerOrderData
    deliveryDate: DateTime
    paymentStatus: PaymentStatus
    paymentMethod: PaymentMethod }

module OrderMapping =
  let toOrderData (order: Order) : OrderData =
    let id =
      if String.IsNullOrEmpty(order.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(order.id)

    { id = id
      occurred = order.occurred
      status = order.status
      customer = order.customer
      deliveryDate = order.deliveryDate
      items = order.items
      paymentMethod = order.paymentMethod
      paymentStatus = order.paymentStatus
      value = order.value }

  let toOrder (data: OrderData) : Order =
    { id = data.id.ToString()
      occurred = data.occurred
      status = data.status
      deliveryDate = data.deliveryDate
      customer = data.customer
      items = data.items
      paymentMethod = data.paymentMethod
      paymentStatus = data.paymentStatus
      value = data.value }
