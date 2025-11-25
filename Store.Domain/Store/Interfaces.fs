module Store.Domain.Store.Interfaces

open Store.Domain.Interfaces
open Store.Domain.Store.Catalog

type IProductTypeRepository =
  inherit IRepository<ProductType>
  abstract member All: unit -> Async<ServiceOutput<ProductType list>>

type IProfessionalRepository =
  inherit IRepository<Professional>
  abstract member All: unit -> Async<ServiceOutput<Professional list>>
  abstract member AllActive: unit -> Async<ServiceOutput<Professional list>>

type ProductSearchCriteria = {
  search:string option
  professionalId:string option
  productTypeId:string option 
}

type IProductRepository =
  inherit IRepository<Product>
  abstract member All: unit -> Async<ServiceOutput<Product list>>
  abstract member Fetch: ProductSearchCriteria -> Async<ServiceOutput<Product list>>

type ISampleRepository =
  inherit IRepository<Sample>
  abstract member SaveAll: Sample * SampleFile -> Async<ServiceOutput<Sample>>
  abstract member Fetch: string -> Async<ServiceOutput<Sample list>>
  abstract member FetchFile: string -> Async<ServiceOutput<SampleFile>>
  abstract member Delete: string -> Async<ServiceOutput<unit>>
