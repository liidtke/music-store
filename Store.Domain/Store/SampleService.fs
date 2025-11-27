module Store.Domain.Store.SampleService

open Store.Domain
open Store.Domain.Store.Catalog


type FetchEffect = FetchSingleEffect<string, Sample>
type FetchProductEffect = FetchSingleEffect<string, Product>
type FetchFileEffect = FetchSingleEffect<string, SampleFile>
type SaveEffect = Sample * SampleFile -> Async<ServiceOutput<Sample>>

type CreateSampleInput =
  { sampleType: SampleType
    description: string
    productId: string
    fileName: string
    mimeType: string
    contents: byte[] }

let private validate (us: UserSession) (product: Product) =
  if us.role = Role.AdminRole || us.userId = product.professionalId then
    succeed product
  else
    invalidate "Access Denied"

let create
  (us: UserSession)
  (fetchProductEffect: FetchProductEffect)
  (saveEffect: SaveEffect)
  (input: CreateSampleInput)
  =
  let toSample (product: Product) =
    ({ id = ""
       sampleType = input.sampleType
       description = input.description
       professionalId = product.professionalId
       productId = product.id },
     { id = ""
       fileName = input.fileName
       mimeType = input.mimeType
       contents = input.contents })
    |> succeed

  async {
    let! productOutput = fetchProductEffect input.productId

    match productOutput with
    | Success product -> return! product |> validate us ?> toSample ?>< saveEffect
    | Failure errorResult -> return invalidate "Product not found"

  }
