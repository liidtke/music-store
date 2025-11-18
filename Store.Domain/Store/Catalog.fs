module Store.Domain.Store.Catalog

type ProductType =
  { id: Id
    name: string
    pictureUrl: string
    isActive: bool
     }
// gravação, mixagem, produção

// type ComboTemplate =
//   { id: Id
//     name: string
//     types: ProductType list
//     createdBy: int }

type Professional =
  { id: Id //same as user id
    name: string
    document: string option
    bulkDiscount: decimal
    isActive: bool
    pixKey:string option //for direct pix payments
    isManager: bool //for managing delivery
    pictureUrl: string option
    //json from editor.js
    profile: string }
  
type Product =
  { id: Id
    professionalId: Id
    productType: ProductType
    description: string option //json from editor.js
    unitPrice: decimal
    oldUnitPrice: decimal
    //effort for the completion of the work, est in hours
    effort: int
    //min days to work be complete
    minDeadLine: int
    isActive: bool }
