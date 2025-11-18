module Store.Infrastructure.Access.Models

open System
open MongoDB.Bson
open Store.Domain

[<CLIMutable>]
type UserData =
  { id: BsonObjectId
    name: string
    email: string
    password: string option
    role: int
    isDeleted: bool
    isVerified: bool }

[<CLIMutable>]
type UserVerificationData =
  { id: BsonObjectId
    userId: BsonObjectId
    hash: string option
    expiresIn: DateTime
    isUsed: bool
    verificationCode: string }

module RoleMapping =
  let toInt (role: Role) =
    match role with
    | AdminRole -> 0
    | ProfessionalRole -> 1
    | CustomerRole -> 2
    | Anon -> 9

  let ofInt (value: int) =
    match value with
    | 0 -> AdminRole
    | 1 -> ProfessionalRole
    | 2 -> CustomerRole
    | 9 -> Anon
    | _ -> Anon

module UserMapping =
  let toUserData (user: User) : UserData =
    let id =
      if String.IsNullOrEmpty(user.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(user.id)

    { id = id
      name = user.name
      email = user.email
      password = user.password
      role = RoleMapping.toInt user.role
      isDeleted = user.isDeleted
      isVerified = user.isVerified }

  let toUser (data: UserData) : User =
    { id = data.id.ToString()
      name = data.name
      email = data.email
      password = data.password
      role = RoleMapping.ofInt data.role
      isDeleted = data.isDeleted
      isVerified = data.isVerified }

module UserVerificationMapping =
  let toUserVerificationData (verification: UserVerification) : UserVerificationData =
    let id =
      if String.IsNullOrEmpty(verification.id) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(verification.id)

    let userId =
      if String.IsNullOrEmpty(verification.userId) then
        BsonObjectId.Empty
      else
        BsonObjectId.Create(verification.userId)

    { id = id
      userId = userId
      hash = verification.hash
      expiresIn = verification.expiresIn
      isUsed = verification.isUsed
      verificationCode = verification.verificationCode }

  let toUserVerification (data: UserVerificationData) : UserVerification =
    { id = data.id.ToString()
      userId = data.userId.ToString()
      hash = data.hash
      user = None
      expiresIn = data.expiresIn
      isUsed = data.isUsed
      verificationCode = data.verificationCode }
