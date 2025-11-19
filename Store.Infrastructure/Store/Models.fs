module Store.Infrastructure.Store.Models


open System
open MongoDB.Bson
open Store.Domain.Store.Catalog

[<CLIMutable>]
type ProductTypeData =
  { id: BsonObjectId
    name: string
    pictureUrl: string
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
      pictureUrl = productType.pictureUrl
      isActive = productType.isActive }

  let toProductType (data: ProductTypeData) : ProductType =
    { id = data.id.ToString()
      name = data.name
      pictureUrl = data.pictureUrl
      isActive = data.isActive }


[<CLIMutable>]
type ProfessionalData =
  { id: BsonObjectId
    name: string
    document: string option
    bulkDiscount: decimal
    isActive: bool
    pixKey: string option
    isManager: bool
    pictureUrl: string option
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
      document = professional.document
      bulkDiscount = professional.bulkDiscount
      isActive = professional.isActive
      pixKey = professional.pixKey
      isManager = professional.isManager
      pictureUrl = professional.pictureUrl
      profile = professional.profile }

  let toProfessional (data: ProfessionalData) : Professional =
    { id = data.id.ToString()
      name = data.name
      document = data.document
      bulkDiscount = data.bulkDiscount
      isActive = data.isActive
      pixKey = data.pixKey
      isManager = data.isManager
      pictureUrl = data.pictureUrl
      profile = data.profile }

[<CLIMutable>]
type ProductData =
  { id: BsonObjectId
    name: string
    professionalId: BsonObjectId
    productType: ProductTypeData
    description: string option
    unitPrice: decimal
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
      description = product.description
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
      description = data.description
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
      description = sample.description
      productId = productId }

  let toSample (data: SampleData) : Sample =
    { id = data.id.ToString()
      sampleType = enum<SampleType> data.sampleType
      description = data.description
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