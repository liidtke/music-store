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
