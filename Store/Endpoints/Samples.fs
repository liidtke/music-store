module Store.Endpoints.Samples

open Application
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Http
open Store
open Store.Domain.Store
open Store.Domain.Store.Catalog
open Store.Domain.Store.Interfaces
open Store.Domain.Store.SampleService
open Store.Endpoints.Queries

type SampleInputModel =
  { productId: string
    sampleType: string
    description: string
    file: IFormFile option }

module SampleType =
  let mapString (s: string) =
    match s with
    | "Audio" -> SampleType.Audio
    | "Image" -> SampleType.Image
    | "Pdf" -> SampleType.Pdf
    | _ -> failwith "Invalid sample type"

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

    async {
      return! sp ?>< SampleService.create stx.userSession db_product.Find db.SaveAll
    }

  let handler: HttpHandler =
    Request.mapForm (fun q ->
      { productId = q.GetString("productId")
        sampleType = q.GetString("productType")
        description = q.GetString("description")
        file = q.TryGetFile("file") })
    <| runAsync workflow

  let endpoint = post "samples" handler

module File =
  let private workflow (stx: ServiceContext) (productId: string) =
    let db = stx.context.Plug<ISampleRepository>()
    db.Fetch(productId)

  let handler: HttpHandler =
    Request.mapRoute (fun q -> q.GetString("productId")) <| runAnonAsync workflow

  let endpoint = get "samples/{id}" handler

let private workflow (stx: ServiceContext) (productId: string) =
  let db = stx.context.Plug<ISampleRepository>()
  db.Fetch(productId)

let handler: HttpHandler =
  Request.mapQuery (fun q -> q.GetString("productId")) <| runAnonAsync workflow

let allEndpoint = get "samples" handler

let endpoints = [ allEndpoint ]
