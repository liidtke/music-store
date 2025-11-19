module Store.Domain.Store.ProfessionalService

open System
open Store.Domain
open Store.Domain.Store.Catalog

type SaveEffect = Effect<Professional>
type FetchEffect = FetchSingleEffect<string, Professional>

let private validateAccess (us: UserSession) (professional: Professional) =
  if us.role = Role.AdminRole then succeed professional
  elif us.userId = professional.id then succeed professional
  else invalidate "Access denied"

let private validation (input: Professional) =
  if input.name = "" then
    invalidate "Name cannot be empty"
  elif input.name.Length > 50 then
    invalidate "Name cannot be longer than 50 characters"
  elif input.bulkDiscount < 0M then
    invalidate "Bulk discount cannot be negative"
  elif input.bulkDiscount > 100M then
    invalidate "Bulk discount cannot be greater than 100%"
  elif String.IsNullOrEmpty(input.profile) then
    invalidate "Profile cannot be empty"
  else
    succeed input

let create (us: UserSession) (saveEffect: SaveEffect) (input: Professional) =
  { input with
      name = input.name.Trim()
      isActive = true }
  |> validation
  ?> validateAccess us
  ?>< saveEffect

let update (us: UserSession) (saveEffect: SaveEffect) (fetchEffect: FetchEffect) (input: Professional) =
  async {
    let! professional = fetchEffect input.id

    return!
      professional
      ?> (fun pro ->
        { pro with
            bulkDiscount = input.bulkDiscount
            name = input.name.Trim()
            document = input.document
            pixKey = input.pixKey
            isManager = input.isManager
            isActive = input.isActive }
        |> validation)
      ?> validateAccess us
      ?>< saveEffect
  }
