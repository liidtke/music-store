[<AutoOpen>]
module Store.Domain.Health.Access

open System

type Role =
  | AdminRole //system admin
  | ProfessionalRole //professional on store
  | CustomerRole
  | Anon //not logged user

type User =
  { id: int
    name: string
    email: string
    password: string option
    role: Role
    isDeleted: bool
    isVerified: bool }

  member this.update user =
    { this with
        name = user.name
        role = user.role
        isVerified = user.isVerified }

type UserVerification =
  { id: int
    userId: int
    hash: string option
    user: User option
    expiresIn: DateTime
    isUsed: bool
    verificationCode: string } //salt

type UserSession =
  { userId: int
    email: string
    name: string
    role: Role }

  member this.HasAccess organizationId =
    this.role = Role.AdminRole || organizationId = organizationId

module UserSession =
  let anon =
    { userId = -1
      email = ""
      name = ""
      role = Role.Anon }
