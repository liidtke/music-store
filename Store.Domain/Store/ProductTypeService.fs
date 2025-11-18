module Store.Domain.Store.ProductTypeService

open Store.Domain.Store.Catalog

type SaveEffect = Effect<ProductType>
type FetchEffect = FetchSingleEffect<string, ProductType>

let private validation (input: ProductType) =
  if input.name = "" then
    invalidate "Name cannot be empty"
  elif input.name.Length > 50 then
    invalidate "Name cannot be longer than 50 characters"
  elif input.pictureUrl = "" then
    invalidate "Picture URL cannot be empty"
  else
    succeed input

let save (saveEffect: SaveEffect) (input: ProductType) =
  { input with
      name = input.name.Trim()
      isActive = true }
  |> validation
  ?>< saveEffect

let update (saveEffect: SaveEffect) (input: ProductType) =
  { input with
      name = input.name.Trim()
      isActive = true }
  |> validation
  ?>< saveEffect
