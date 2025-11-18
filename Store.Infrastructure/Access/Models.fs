module Store.Infrastructure.Access.Models

open Store.Domain

type UserData =
  { id: string
    name: string
    email: string
    password: string option
    role: int
    isDeleted: bool
    isVerified: bool }

module RoleMapping =
  let toInt (role: Role) =
    match role with
    | AdminRole        -> 0
    | ProfessionalRole -> 1
    | CustomerRole     -> 2
    | Anon             -> 9

  let ofInt (value: int) =
    match value with
    | 0 -> AdminRole
    | 1 -> ProfessionalRole
    | 2 -> CustomerRole
    | 9 -> Anon
    | _ -> Anon   

module UserMapping =
  let toUserData (user: User) : UserData =
    { id = user.id
      name = user.name
      email = user.email
      password = user.password
      role = RoleMapping.toInt user.role
      isDeleted = user.isDeleted
      isVerified = user.isVerified }

  let toUser (data: UserData) : User =
    { id = data.id
      name = data.name
      email = data.email
      password = data.password
      role = RoleMapping.ofInt data.role
      isDeleted = data.isDeleted
      isVerified = data.isVerified }
