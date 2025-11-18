module Store.Domain.Store.Interfaces

open Store.Domain.Interfaces
open Store.Domain.Store.Catalog

type IProductTypeRepository =
  inherit IRepository<ProductType>
  abstract member All: unit -> Async<ServiceOutput<ProductType list>>

type IProfessionalRepository =
  inherit IRepository<Professional>
  abstract member All: unit -> Async<ServiceOutput<Professional list>>
