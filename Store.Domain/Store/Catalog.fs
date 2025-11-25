module Store.Domain.Store.Catalog

open System

type ProductType =
  { id: Id
    name: string
    pictureUrl: string
    isActive: bool }
// gravação, mixagem, produção

// type ComboTemplate =
//   { id: Id
//     name: string
//     types: ProductType list
//     createdBy: int }

type Professional =
  { id: Id //same as user id
    name: string
    title: string
    document: string
    bulkDiscount: decimal
    isActive: bool
    pixKey: string option //for direct pix payments
    isManager: bool //for managing delivery
    pictureUrl: string option
    //json from editor.js
    profile: string }

type Product =
  { id: Id
    name:string
    professionalId: Id
    professionalName: string
    productType: ProductType
    description: string option //json from editor.js
    unitPrice: decimal
    oldUnitPrice: decimal
    //effort for the completion of the work, est in hours
    effort: int
    //min days to work be complete
    minDeadLine: int
    isActive: bool }

type SampleType =
  | Audio = 1
  | Pdf = 2
  | Video = 3
  | Image = 4

type Sample =
  { id: Id
    sampleType: SampleType
    description: string
    productId: Id
    }
  
type SampleFile = {
    id:Id //same as sample
    fileName: string
    mimeType: string
    contents: byte array }

//v1
type CreatedBy = { userId: string; userName: string }

type QuestionAsked =
  { id: Id
    professionalId: Id
    question: string
    response: string option
    isPending: bool
    occurred: DateTime
    createdBy: CreatedBy }
