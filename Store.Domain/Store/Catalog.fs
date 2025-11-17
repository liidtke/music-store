module Store.Domain.Store.Catalog

type ProductType =
  { id: Id
    name: string
    pictureUrl: string
    
     }
// gravação, mixagem, produção

// type ComboTemplate =
//   { id: Id
//     name: string
//     types: ProductType list
//     createdBy: int }

//portfolio
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

type Professional =
  { id: Id
    name: string
    document: string
    userId: int
    bulkDiscount: decimal
    isDeleted: bool
    pixKey:string option //for direct pix payments
    isManager: bool //for managing delivery
    pictureUrl: string option
    //json from editor.js
    profile: string }
