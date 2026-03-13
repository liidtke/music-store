module Store.Endpoints.Samples

open System.IO
open Application
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Http
open Store
open Store.Domain.Store
open Store.Domain.Store.Catalog
open Store.Domain.Store.Interfaces
open Store.Domain.Store.SampleService
open Store.Endpoints.Models
open Store.Endpoints.Queries
open System.Threading.Tasks

type SampleInputModel =
  { productId: string
    sampleType: string
    description: string
    file: IFormFile option }


module Create =
  let private workflow (stx: ServiceContext) (input: SampleInputModel) =
    let db = stx.context.Plug<ISampleRepository>()
    let db_product = stx.context.Plug<IProductRepository>()

    let readFile (file: IFormFile) =
      use ms = new System.IO.MemoryStream()
      file.CopyTo(ms)

      { productId = input.productId
        sampleType = SampleType.mapString input.sampleType
        description = input.description
        fileName = file.FileName
        mimeType = file.ContentType
        contents = ms.ToArray() }
      |> succeed

    let sp =
      match input.file with
      | None -> invalidate "File not found"
      | Some value -> readFile value

    async { return! sp ?>< SampleService.create stx.userSession db_product.Find db.SaveAll }

  let handler: HttpHandler =
    Request.mapForm (fun q ->
      { productId = q.GetString("productId")
        sampleType = q.GetString("productType")
        description = q.GetString("description")
        file = q.TryGetFile("file") })
    <| runAsync workflow

  let endpoint = post "samples" handler

module File =

  let private workflow (sampleId: string) : HttpHandler =
    fun (ctx: HttpContext) ->
      let db = ctx.Plug<ISampleRepository>()

      let s (file: SampleFile) =
        task {
          ctx.Response.Headers.Append("Content-Disposition", $"inline; filename=\"{file.fileName}\"")
          ctx.Response.ContentType <- file.mimeType
          do! ctx.Response.Body.WriteAsync(file.contents, 0, file.contents.Length)
          return Response.ofJson file
        }

      task {
        let! fileOpt = db.FetchFile(sampleId)

        match fileOpt with
        | Failure errorResult -> return Application.errorHandler errorResult
        | Success sampleFile -> return! s sampleFile
      }

  let handler: HttpHandler = Request.mapRoute (fun q -> q.GetString("id")) workflow


  let endpoint = get "/samples/{id}" handler

// module File =
//   let private workflow (stx: ServiceContext) (productId: string) =
//     let db = stx.context.Plug<ISampleRepository>()
//     db.FetchFile(productId)
//
//   let handler: HttpHandler =
//     Request.mapRoute (fun q -> q.GetString("productId")) <| runAnonAsync workflow
//
//   let endpoint = get "samples/{id}" handler
//
// let private workflow (stx: ServiceContext) (input) =
//   let db = stx.context.Plug<ISampleRepository>()
//
//   async {
//     let! result = db.Fetch(input)
//     return result ?> (fun i -> i |> List.map SampleModel.map |> succeed)
//   }

let private workflow (stx: ServiceContext) (input) =
  let db = stx.context.Plug<ISampleRepository>()

  async {
    let! result = db.Fetch(input)
    return result ?> (fun i -> i |> List.map SampleModel.map |> succeed)
  }

let handler: HttpHandler =
  Request.mapQuery (fun q ->
    { productId = q.TryGetString("productId") |> fromString
      professionalId = q.TryGetString("professionalId") |> fromString })
  <| runAnonAsync workflow

let allEndpoint = get "samples" handler

let endpoints = [ allEndpoint; Create.endpoint; File.endpoint ] //todo delete
