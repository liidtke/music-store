module Store.Domain.Store.CustomerService

open System
open Store.Domain
open Store.Domain.Store.Ordering

type SaveEffect = Effect<Customer>
type FetchEffect = FetchSingleEffect<string, Customer>
type FetchUserEffect = FetchSingleEffect<string, User>

type CustomerServiceInput =
  { id: Id
    name: string
    phone: Phone
    address: Address option
    document: string }

let private maxStringSize = 200

let private validateStringField (fieldName: string) (value: string) =
  if String.IsNullOrEmpty(value) then
    invalidate $"{fieldName} cannot be null or empty"
  elif value.Length > maxStringSize then
    invalidate $"{fieldName} cannot be greater than {maxStringSize} characters"
  else
    succeed value

let private validatePhone (phone: Phone) =
  validateStringField "Phone number" phone.number ?> (fun _ -> succeed phone)

let private validateAddress (address: Address) =
  let validations =
    [ validateStringField "Street" address.street
      validateStringField "Number" address.number
      validateStringField "ZipCode" address.zipCode
      validateStringField "City" address.city
      validateStringField "State" address.state ]

  let errors =
    validations
    |> List.choose (fun v ->
      match v with
      | Failure e -> Some e.errorMessages
      | Success _ -> None)
    |> List.concat

  if errors.IsEmpty then
    succeed address
  else
    Failure
      { errorType = InvalidInput
        ex = None
        errorMessages = errors }

let private validateCustomer (input: Customer) =
  succeed input
  //todo
 

let private validateUserExists (userResult: ServiceOutput<User>) (customer: Customer) =
  match userResult with
  | Success _ -> succeed customer
  | Failure _ -> invalidate "User not found. Customer ID must be a valid user ID"

let create (us: UserSession) (saveEffect: SaveEffect) (fetchUserEffect: FetchUserEffect) (input: CustomerServiceInput) =
  let customer: Customer =
    { id = input.id
      name = input.name.Trim()
      phone = input.phone
      address = input.address
      document = input.document.Trim() }

  let work =
    async {
      let! userResult = fetchUserEffect input.id

      return! customer |> validateUserExists userResult ?> validateCustomer ?>< saveEffect
    }

  if us.role.IsAdminRole || us.role.IsProfessionalRole || us.userId = input.id then
    work
  else
    async { return invalidate "Unauthorized" }

let update
  (us: UserSession)
  (saveEffect: SaveEffect)
  (fetchEffect: FetchEffect)
  (fetchUserEffect: FetchUserEffect)
  (input: CustomerServiceInput)
  =
  let updateCustomer (existing: Customer) =
    { existing with
        name = input.name.Trim()
        phone = input.phone
        address = input.address
        document = input.document.Trim() }
    |> succeed

  let work =
    async {
      let! existingResult = fetchEffect input.id
      let! userResult = fetchUserEffect input.id

      return!
        existingResult
        ?> validateUserExists userResult
        ?> updateCustomer
        ?> validateCustomer
        ?>< saveEffect
    }

  if us.role.IsAdminRole || us.role.IsProfessionalRole || us.userId = input.id then
    work
  else
    async { return invalidate "Unauthorized" }
