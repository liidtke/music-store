module Store.Domain.Store.ProductService

open System
open Store.Domain
open Store.Domain.Store.Catalog

let private validateProduct (input: Product) =
  if input.unitPrice < 0M then
    invalidate "Price cannot be negative"
  elif input.oldUnitPrice < 0M then
    invalidate "Price cannot be negative"
  elif String.IsNullOrEmpty(input.name) then
    invalidate "Name cannot be empty"
  elif input.minDeadLine < 0 then
    invalidate "DeadLine cannot be negative"
  elif input.effort < 0 then
    invalidate "Effort cannot be negative"
  else
    succeed input

let private validateAccess (us: UserSession) (input: Product) =
  if us.role = AdminRole then
    succeed input
  elif us.role = ProfessionalRole && input.professionalId = us.userId then
    succeed input
  else
    invalidate "Access denied"

type SaveEffect = Effect<Product>
type FetchEffect = FetchSingleEffect<string, Product>
type FetchProductType = FetchSingleEffect<string, ProductType>
type FetchProfessional = FetchSingleEffect<string, Professional>

type ProductServiceInput =
  { id: Id
    name: string
    professionalId: Id
    productTypeId: Id
    description: string option
    unitPrice: decimal
    oldUnitPrice: decimal
    imageUrl: string option
    effort: int
    minDeadLine: int
    isActive: bool }

let create
  (us: UserSession)
  (saveEffect: SaveEffect)
  (fetchProductType: FetchProductType)
  (fetchProfessional: FetchProfessional)
  (input: ProductServiceInput)
  =
  let toProduct (pt: ProductType) (professional: Professional) =
    { id = input.id
      effort = input.effort
      name = input.name
      minDeadLine = input.minDeadLine
      professionalName = professional.name
      imageUrl = input.imageUrl
      productType = pt
      oldUnitPrice = input.oldUnitPrice
      unitPrice = input.unitPrice
      description = input.description
      isActive = true
      professionalId = input.professionalId }
    |> succeed

  async {
    let! pt = fetchProductType input.productTypeId
    let! professional = fetchProfessional input.professionalId

    return!
      may2 toProduct pt professional ?> validateAccess us ?> validateProduct
      ?>< saveEffect
  }

let update
  (us: UserSession)
  (saveEffect: SaveEffect)
  (fetchEffect: FetchEffect)
  (fetchProductType: FetchProductType)
  (fetchProfessional: FetchProfessional)
  (input: ProductServiceInput)
  =
  let toProduct (pt: ProductType) (professional: Professional) =
    { id = input.id
      effort = input.effort
      name = input.name
      minDeadLine = input.minDeadLine
      productType = pt
      professionalName = professional.name
      imageUrl = input.imageUrl
      oldUnitPrice = input.oldUnitPrice
      unitPrice = input.unitPrice
      description = input.description
      isActive = input.isActive
      professionalId = input.professionalId }
    |> succeed

  let update (existing: Product) (input: Product) =
    { existing with
        name = input.name
        minDeadLine = input.minDeadLine
        effort = input.effort
        productType = existing.productType
        imageUrl = input.imageUrl
        unitPrice = input.unitPrice
        professionalName = input.professionalName
        oldUnitPrice = input.oldUnitPrice
        description = input.description
        isActive = input.isActive }
    |> succeed

  async {
    let! pt = fetchProductType input.productTypeId
    let! existing = fetchEffect input.id
    let! professional = fetchProfessional input.professionalId

    return!
      may2 toProduct pt professional |> may2 update existing
      ?> validateAccess us
      ?> validateProduct
      ?>< saveEffect
  }
