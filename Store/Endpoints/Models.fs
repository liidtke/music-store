module Store.Endpoints.Models

open System
open Store.Domain.Health
open Models

type UserModel =
  { id: int
    name: string
    email: string
    role: string
    isDeleted: bool
    isVerified: bool }


module UserModel =
  let map (user: User) =
    { id = user.id
      name = user.name
      email = user.email
      role = Role.toString user.role
      isDeleted = user.isDeleted
      isVerified = user.isVerified }



module UserSession =
  type UserSessionModel =
    { userId: int
      email: string
      name: string
      role: string }

  let map (us: UserSession) : UserSessionModel =
    { userId = us.userId
      email = us.email
      name = us.name
      role = Role.toString us.role }
