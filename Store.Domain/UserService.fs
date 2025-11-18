module Store.Domain.Health.UserService

open Store.Domain.Access
open Store.Domain.Interfaces

module Effects =
  type CountByEmail = string -> Async<ServiceOutput<int>>
  type SaveUser = Effect<User>
  type SearchUser = string -> Async<ServiceOutput<User list>>
  type GetUser = string -> Async<ServiceOutput<User>>
  type GetUserByEmail = string -> Async<ServiceOutput<User>>
  type EncryptPassword = string -> string

  type Dependencies =
    { count: CountByEmail
      save: SaveUser
      get: GetUser }


module private Validations =
  let hasAccessToCreate (current: UserSession) (user: User) = succeed user

  let validName (user: User) =
    if user.name <> null && user.name <> "" && user.name.Length < 200 then
      succeed user
    else
      invalidate "Nome Inválido"

  let validPassword user =
    if user.password.IsSome && user.password.Value.Length > 5 then
      succeed user
    else
      invalidate "Senha Inválida, deve conter mais que 5 caracteres"

  let validEmail (user: User) =
    if
      user.email <> null
      && user.email <> ""
      && user.email.Contains("@")
      && user.email.Length < 100
    then
      succeed user
    else
      invalidate "Email Inválido"

  let hasDuplicate (effect: Effects.CountByEmail) (user: User) =
    async {
      let! countOutput = effect user.email

      match countOutput with
      | Success c ->
        if c = 0 then
          return succeed user
        else
          return invalidate "Um usuário com esse email já existe"
      | Failure errorResult -> return invalidate "Um usuário com esse email já existe"
    }

  let updateValidate (current: UserSession) (newValue: User) (oldValue: User) =
    let changedEmail =
      if newValue.email <> oldValue.email then
        invalidate "Não é possível alterar o email"
      else
        succeed newValue


    let roleValidation =
      if
        newValue.role <> oldValue.role
        && (current.role <> Role.AdminRole && (newValue.role = Role.AdminRole)) //todo testar
      then
        invalidate "Não é possível alterar o perfil"
      else
        succeed newValue

    changedEmail |> join roleValidation

open Effects
open Validations

let private saveAsync u =
  async {
    do! Async.Sleep(100)
    return succeed u
  }

module Create =

  type CreateUser =
    { name: string
      email: string
      password: string
      role: string }

  let handle (current: UserSession) (enc: EncryptPassword) (repository: IUserRepository) (input: CreateUser) =
    let toUser input =
      if input.password = "" then
        invalidate "Senha Inválida"
      else
        { id = ""
          name = input.name
          email = input.email
          password =
            if input.password <> null then
              input.password |> enc |> Some
            else
              None
          role = Role.CustomerRole
          isDeleted = false
          isVerified = false }
        |> succeed

    input |> toUser ?> validEmail ?> hasAccessToCreate current ?> validName
    ?>< hasDuplicate repository.Count
    ?<> repository.Add


module Update =

  let handle (current: UserSession) (repository: IUserRepository) (modified: User) =
    let validate o =
      updateValidate current modified o ?> validName

    let alter (m: User) (o: User) =
      let toSave = o.update m
      toSave |> succeed

    async {
      let! original = repository.Find modified.id
      return! original ?> validate ?> alter modified ?>< repository.Update
    }

  let changePassword
    (email: string)
    (newPassword: string)
    (saveEffect: SaveUser)
    (getEffect: GetUserByEmail)
    (enc: EncryptPassword)
    =
    async {
      let! user = getEffect email

      return!
        user
        ?> (fun u -> { u with password = Some(newPassword) } |> succeed)
        ?> validPassword
        ?> (fun u ->
          { u with
              password = Some(enc newPassword) }
          |> succeed)
        ?>< saveEffect
    }
