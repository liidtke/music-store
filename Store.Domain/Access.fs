[<AutoOpen>]
module Store.Domain.Access

open System

type Role =
  | AdminRole //system admin
  | ProfessionalRole //professional on store
  | CustomerRole
  | Anon //not logged user

[<CLIMutable>]
type User =
  { id: string
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
  { id: string
    userId: string
    hash: string option
    user: User option
    expiresIn: DateTime
    isUsed: bool
    verificationCode: string } //salt

type UserSession =
  { userId: string
    email: string
    name: string
    role: Role }

  member this.HasAccess organizationId =
    this.role = Role.AdminRole || organizationId = organizationId

module UserSession =
  let anon =
    { userId = ""
      email = ""
      name = ""
      role = Role.Anon }
