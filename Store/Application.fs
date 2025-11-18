module Application

open System
open System.IO
open System.Text.Json
open Falco
open System.Collections.Generic
open Store
open Store.Domain
open Store.Domain.Health
open MemoryPack
open Microsoft.AspNetCore.Authentication
open Models
open SharedKernel
open Microsoft.AspNetCore.Http
open Falco.Markup
open FSharp.SystemTextJson
open System.Text.Json
open System.Text.Json.Serialization


let private setContentLength (contentLength: int64) (ctx: HttpContext) =
  ctx.Response.ContentLength <- contentLength

let private writeStream (str: Stream) : HttpHandler =
  fun ctx ->
    task {
      setContentLength str.Length ctx
      str.Position <- 0
      do! str.CopyToAsync(ctx.Response.Body)
    }

let ofJson2 (obj: 'T) : HttpHandler =
  fun ctx ->
    task {
      use str = new MemoryStream()
      do! MemoryPackSerializer.SerializeAsync(str, obj)

      return!
        Response.withContentType "application/json; charset=utf-8" ctx
        |> writeStream str
    }

let handleInvalidAuth: HttpHandler =
  Response.withStatusCode 401 >> Response.ofPlainText "Unauthorized"

let secureWithScope (ok: HttpHandler) : HttpHandler = Request.ifAuthenticated "Bearer" ok

let secure (ok: HttpHandler) : HttpHandler = Request.ifAuthenticated "HsScheme" ok

let secureHandler (ok: HttpHandler) : HttpHandler =
  fun (ctx: HttpContext) ->
    task {
      let! authenticateResult = ctx.AuthenticateAsync("HsScheme")

      if authenticateResult.Succeeded then
        return! ok ctx
      else
        return! Response.redirectPermanently "/login" ctx
    }

let notSecure (ok: HttpHandler) : HttpHandler =
  Request.ifNotAuthenticated "HsScheme" ok

// let unsecure: HttpHandler = Request.ifNotAuthenticated "HsScheme" handleInvalidAuthHtml

let jsonOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)

let jsonOptionsFsharp =
  JsonFSharpOptions()
    .WithUnionExternalTag()
    .WithUnionUnwrapFieldlessTags()
    .WithUnwrapOption()
    .WithUnionUnwrapSingleFieldCases()
    .WithUnionAllowUnorderedTag()
    .ToJsonSerializerOptions()
//        https://github.com/Tarmil/FSharp.SystemTextJson/blob/master/docs/Customizing.md#global-options

//output
let errorStatusCode errorType =
  match errorType with
  | NotFound -> Response.withStatusCode 404
  | Validation -> Response.withStatusCode 409
  | InvalidInput -> Response.withStatusCode 400
  | InternalError -> Response.withStatusCode 500
  | Forbidden -> Response.withStatusCode 403

let errorHandler (serviceError: ErrorResult) : HttpHandler =
  errorStatusCode serviceError.errorType
  >> Response.ofPlainText serviceError.Message

let handleJson error : HttpHandler =
  let message = sprintf "Invalid JSON: %s" error

  Response.withStatusCode 400 >> Response.ofPlainText message

let authHandler: HttpHandler =
  Response.withStatusCode 401 >> Response.ofPlainText "Unauthorized"

let from (serviceOutput: ServiceOutput<'a>) =
  let returnIt a =
    match box a with
    | :? XmlNode as node -> Response.ofHtml node
    | _ -> a |> Response.ofJsonOptions jsonOptionsFsharp

  match serviceOutput with
  | Success result -> returnIt result
  | Failure e -> (errorHandler e)

let badRequest text =
  Response.withStatusCode 400 >> Response.ofPlainText text

//system
type SystemUser =
  { userId: Guid
    userName: string
    isAdmin: bool }

module SystemUser =
  let getIntValues (headers: IDictionary<string, string>) name =
    if headers.ContainsKey(name) && headers[name] <> "" then
      try
        headers[name].Split(",") |> List.ofArray |> List.map (fun x -> x |> int)
      with e ->
        List.empty
    else
      List.Empty

  let getStringValues (headers: IDictionary<string, string>) name =
    if headers.ContainsKey(name) then
      headers[name].Split(",") |> List.ofArray
    else
      List.Empty

  let standard =
    { userId = Guid.Empty
      userName = "System"
      isAdmin = true }

type ServiceHandler<'input, 'output> = 'input -> ServiceOutput<'output>
type AsyncServiceHandler<'input, 'output> = 'input -> Async<ServiceOutput<'output>>

let setHeaders (ctx: HttpContext) (us: UserSession) : unit =
  ctx.Response.Headers.Add("x-us-id", us.userId.ToString())
  ctx.Response.Headers.Add("x-us-email", us.email)
  ctx.Response.Headers.Add("x-us-role", Role.toString us.role)

let run (serviceHandler: ServiceContext -> ServiceHandler<'input, 'output>) (input: 'input) : HttpHandler =
  fun (ctx: HttpContext) ->
    let so = getServiceContext true ctx |> Async.RunSynchronously

    match so with
    | Success deps ->
      setHeaders ctx deps.userSession
      from (serviceHandler <| deps <| input) ctx
    | Failure f -> authHandler ctx

let runAnonAsync
  (serviceHandler: ServiceContext -> AsyncServiceHandler<'input, 'output>)
  (input: 'input)
  : HttpHandler =
  fun (ctx: HttpContext) ->
    task {
      let! so = getServiceContext false ctx

      match so with
      | Success deps ->
        setHeaders ctx deps.userSession
        let! result = serviceHandler <| deps <| input
        return! from result ctx
      | Failure f -> return! authHandler ctx
    }

let runAsync (serviceHandler: ServiceContext -> AsyncServiceHandler<'input, 'output>) (input: 'input) : HttpHandler =
  fun (ctx: HttpContext) ->
    task {
      let! so = getServiceContext true ctx

      match so with
      | Success deps ->
        setHeaders ctx deps.userSession
        let! result = serviceHandler <| deps <| input
        return! from result ctx
      | Failure f -> return! authHandler ctx
    }
