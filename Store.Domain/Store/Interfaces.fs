module Store.Domain.Store.Interfaces

open Store.Domain.Interfaces
open Store.Domain.Store.Catalog
open Store.Domain.Store.Entities

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
  limit: int
}

type IProductRepository =
  inherit IRepository<Product>
  abstract member All: unit -> Async<ServiceOutput<Product list>>
  abstract member Fetch: ProductSearchCriteria -> Async<ServiceOutput<Product list>>

type SampleSearchCriteria = {
  professionalId:string option
  productId:string option
}

type ISampleRepository =
  inherit IRepository<Sample>
  abstract member SaveAll: Sample * SampleFile -> Async<ServiceOutput<Sample>>
  abstract member Fetch: SampleSearchCriteria -> Async<ServiceOutput<Sample list>>
  abstract member FetchFile: string -> Async<ServiceOutput<SampleFile>>
  abstract member Delete: string -> Async<ServiceOutput<unit>>
  
  
type ICustomerRepository =
  inherit IRepository<Customer>

type IBasketRepository =
  inherit IRepository<Basket>
