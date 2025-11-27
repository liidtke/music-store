module Store.Endpoints.Models

open System
open Store.Domain
open Store.Domain.Health
open Models
open Store.Domain.Store.Catalog

type UserModel =
  { id: string
    name: string
    email: string
    role: string
    isActive: bool
    isVerified: bool }


module UserModel =
  let map (user: User) =
    { id = user.id
      name = user.name
      email = user.email
      role = Role.toString user.role
      isActive = user.isActive
      isVerified = user.isVerified }



module UserSession =
  type UserSessionModel =
    { userId: string
      email: string
      name: string
      role: string }

  let map (us: UserSession) : UserSessionModel =
    { userId = us.userId
      email = us.email
      name = us.name
      role = Role.toString us.role }

type ProfessionalPublicModel =
  { id: string
    name: string
    imageUrl: string option
    title: string
    profile: string }

module ProfessionalPublicModel =
  let map (p: Professional) : ProfessionalPublicModel =
    { id = p.id
      name = p.name
      imageUrl = p.imageUrl
      title = p.title
      profile = p.profile }

// let toDomain (m: ProfessionalPublicModel) : Professional =
//   { id = m.id
//     name = m.name
//     document = m.document
//     bulkDiscount = m.bulkDiscount
//     isActive = m.isActive
//     pixKey = m.pixKey
//     isManager = m.isManager
//     imageUrl = m.imageUrl
//     profile = m.profile }


module SampleType =
  let mapString (s: string) =
    match s with
    | "Audio" -> SampleType.Audio
    | "Image" -> SampleType.Image
    | "Pdf" -> SampleType.Pdf
    | _ -> SampleType.Audio

  let map (s: SampleType) =
    match s with
    | SampleType.Audio -> "Audio"
    | SampleType.Pdf -> "Pdf"
    | SampleType.Video -> "Video"
    | SampleType.Image -> "Image"
    | _ -> "Audio"

type SampleModel =
  { id: string
    sampleType: string
    description: string
    productId: string
    professionalId: string }


module SampleModel =
  let map (s: Sample) : SampleModel =
    { id = s.id
      sampleType = SampleType.map s.sampleType
      description = s.description
      productId = s.productId
      professionalId = s.professionalId }
