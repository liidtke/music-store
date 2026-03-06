[<AutoOpen>]
module SharedKernel

open System
open System.Threading.Tasks
open Falco.Markup
open Microsoft.Extensions.Logging

type ErrorType =
  | NotFound
  | Validation
  | InvalidInput
  | InternalError
  | Forbidden

type ErrorResult =
  { errorType: ErrorType
    ex: Exception option
    errorMessages: string list }

  member this.Message = String.concat "\n" this.errorMessages

type ServiceOutput<'TSuccess> =
  | Success of 'TSuccess
  | Failure of ErrorResult

let succeed a = Success a

let internalError a =
  Failure
    { errorType = ErrorType.Validation
      ex = None
      errorMessages = [] }

let fail a =
  Failure
    { errorType = ErrorType.InternalError
      ex = None
      errorMessages = [ a ] }

let failure err = Failure err

let invalidate message =
  ServiceOutput.Failure
    { errorType = ErrorType.InvalidInput
      ex = None
      errorMessages = [ message ] }

let forbid message =
  Failure
    { errorType = ErrorType.Forbidden
      ex = None
      errorMessages = [ message ] }

let fromError errorType errorMessage =
  Failure
    { errorType = errorType
      ex = None
      errorMessages = [ errorMessage ] }

let fromException (ex: Exception) =
  Failure
    { errorType = InternalError
      ex = ex |> Some
      errorMessages = [ ex.Message ] }

let validation<'t> message =
  fromError Validation message :> ServiceOutput<'t>

let validationAsync<'t> message =
  async { return fromError Validation message :> ServiceOutput<'t> }

let notFound message = fromError NotFound message

let formValidation html =
  Failure
    { errorType = InvalidInput
      ex = None
      errorMessages = [] }

let addMessage (message: string) (serviceOutput: ServiceOutput<'a>) =
  match serviceOutput with
  | Success res ->
    Failure
      { errorType = ErrorType.Validation
        ex = None
        errorMessages = [ message ] }
  | Failure e ->
    Failure
      { errorType = e.errorType
        ex = None
        errorMessages = e.errorMessages @ [ message ] }

//add one service output to another
let join (s1: ServiceOutput<'a>) (s2: ServiceOutput<'a>) =
  match s1, s2 with
  | Success service1, Success service2 -> s2
  | Success service1, Failure e2 -> s2
  | Failure e1, Success service2 -> s1
  | Failure e1, Failure e2 ->
    Failure
      { errorType = e2.errorType
        ex = None
        errorMessages = e1.errorMessages @ e2.errorMessages }

let newOutput (obj: 'b) (s1: ServiceOutput<'a>) =
  match s1 with
  | Success a -> succeed obj
  | Failure error -> fromError error.errorType error.Message

/// apply func if success (similar to either)
let may func serviceOutput =
  match serviceOutput with
  | Success s -> func s
  | Failure e -> failure e

let perhaps func serviceOutput =
  match serviceOutput with
  | Success s -> func s |> succeed
  | Failure e -> failure e

/// apply func if success, last func is async
let mayAsync asyncFunc serviceOutput =
  match serviceOutput with
  | Success s -> asyncFunc s
  | Failure e -> async { return failure e }

let mayAsync2 asyncFunc serviceOutput =
  async {
    match! serviceOutput with
    | Success s -> return! asyncFunc s
    | Failure e -> return failure e
  }

let mayAsync3 func serviceOutput =
  async {
    match! serviceOutput with
    | Success s -> return func s
    | Failure e -> return failure e
  }

/// execute next function with output if success
let (?>) so func = may func so
let (?><) so func = mayAsync func so
let (?<>) so func = mayAsync2 func so
let (?<) so func = mayAsync3 func so

//similar to successfully
/// continue execution ignoring previous output
let (&?>) output func =
  match output with
  | Success s -> func
  | Failure e -> output

/// compose two functions, only executes second if success
let (&>) s1 s2 = s1 >> may s2

let tryTo f exnHandler x =
  try
    f x |> ignore
    succeed x
  with ex ->
    exnHandler ex

let tryWith f successFunc exnHandler =
  try
    f |> successFunc
  with ex ->
    exnHandler ex

/// map one track function into two preserving output similar to map
let maybe f output = may (f >> succeed) output

/// mas option type to service output
let someToOutput a =
  match a with
  | None -> notFound "not found"
  | Some s -> succeed s

///map a bool function to service output with custom validation message
let mapIf f input err =
  match f input with
  | true -> succeed input
  | false -> err

/// map a bool validation to service output or runs func with param
let mayIf v msg f input =
  match v input with
  | true -> f input
  | false -> invalidate msg

//adapted from recipe prefer using previous functions

//https://fsharpforfunandprofit.com/posts/recipe-part3/

/// apply either a success function or failure function
let either successFunc failureFunc input =
  match input with
  | Success s -> successFunc s
  | Failure e -> failureFunc e

/// convert a switch function into a two-track function
let bind f = either f internalError

/// pipe a two-track value into a switch function
let (>>=) x f = bind f x

/// compose two switches into another switch
let (>=>) s1 s2 = s1 >> bind s2

/// convert a one-track function into a switch
let switch f = f >> succeed

/// convert a one-track function into a two-track function
// let map f =
//    either (f >> succeed) fail

let tee f x =
  f x
  x

// convert a one-track function into a switch with exception handling
let tryCatch f exnHandler x =
  try
    f x |> succeed
  with ex ->
    exnHandler ex |> internalError

// convert two one-track functions into a two-track function
let doubleMap successFunc failureFunc =
  either (successFunc >> succeed) (failureFunc >> internalError)

//fsharp helpers
let isNullOrEmpty str = String.IsNullOrEmpty(str)

let parseGuid (str: string) =
  try
    Some(Guid.Parse str)
  with ex ->
    None

type Effect<'a> = 'a -> Async<ServiceOutput<'a>>
type FetchEffect<'input, 'a> = 'input -> Async<ServiceOutput<'a list>>
type FetchSingleEffect<'input, 'a> = 'input -> Async<ServiceOutput<'a>>

let extract (sos: ServiceOutput<'a> list) =
  if sos |> List.forall (fun o -> o.IsSuccess) then
    sos
    |> List.map (fun o ->
      match o with
      | Success s -> s
      | Failure e -> raise (Exception("Invalid case")))
  else
    []

let raiseOutput (o: 't ServiceOutput) =
  match o with
  | Success t -> ()
  | Failure errorResult ->
    if errorResult.ex.IsSome then
      raise errorResult.ex.Value
    else
      ()


let fromString (str: string option) =
  match str with
  | None -> None
  | Some value ->
    match value with
    | "" -> None
    | "undefined" -> None
    | "null" -> None
    | _ -> Some value

let stringToDisplay (str: string option) =
  match str with
  | None -> ""
  | Some value -> value

let maybeMap (func: 'a -> 'b) (value: Option<'a>) =
  match value with
  | None -> None
  | Some value -> func value |> Some

let maybeMapSo (func: 'a -> 'b) (value: Option<'a>) =
  match value with
  | None -> None |> succeed
  | Some s -> func s |> Some |> succeed

let optionMap (func: 'a -> 'b option) (value: 'a option) =
  match value with
  | None -> None
  | Some item -> func item


// let optionMapSo func so =
//   match so with
//   | Success s -> optionMap func s |> succeed
//   | Failure e -> failure e

/// Apply a function to the success values of two service outputs, short-circuiting on the first failure.
let may2 (f: 'a -> 'b -> ServiceOutput<'c>) (so1: ServiceOutput<'a>) (so2: ServiceOutput<'b>) : ServiceOutput<'c> =
  match so1, so2 with
  | Success a, Success b -> f a b
  | Success _, Failure e -> Failure e
  | Failure e, Success _ -> Failure e
  | Failure e1, Failure e2 ->
    Failure
      { errorType = e1.errorType
        ex = None
        errorMessages = e1.errorMessages @ e2.errorMessages }

let map2Async f (so1: ServiceOutput<'a>) (so2: ServiceOutput<'b>) =
  match so1, so2 with
  | Success a, Success b -> f a b
  | Success _, Failure e -> async { return Failure e }
  | Failure e, Success _ -> async { return Failure e }
  | Failure e1, Failure e2 ->
    async {
      return
        Failure
          { errorType = e1.errorType
            ex = None
            errorMessages = e1.errorMessages @ e2.errorMessages }
    }

let map3 f (so1: ServiceOutput<'a>) (so2: ServiceOutput<'b>) (so3: ServiceOutput<'c>) =
  match so1, so2, so3 with
  | Success a, Success b, Success c -> f a b c
  | Success _, Failure e, _ -> Failure e
  | Failure e, Success _, _ -> Failure e
  | _, _, Failure e -> Failure e
  | Failure e1, Failure e2, Success _ ->
    Failure
      { errorType = e1.errorType
        ex = None
        errorMessages = e1.errorMessages @ e2.errorMessages }
  | Failure e1, Success _, Failure e3 ->
    Failure
      { errorType = e1.errorType
        ex = None
        errorMessages = e1.errorMessages @ e3.errorMessages }
  | Success _, Failure e2, Failure e3 ->
    Failure
      { errorType = e2.errorType
        ex = None
        errorMessages = e2.errorMessages @ e3.errorMessages }
  | Failure e1, Failure e2, Failure e3 ->
    Failure
      { errorType = e1.errorType
        ex = None
        errorMessages = e1.errorMessages @ e2.errorMessages @ e3.errorMessages }

let fromOption result =
  result
  ?> (fun item ->
    match item with
    | None -> notFound "Item não encontrado"
    | Some value -> value |> succeed)


let optionToString (s: string option) =
  match s with
  | None -> ""
  | Some value -> value

type DomainOutput<'T, 'TEvent> =
  { result: ServiceOutput<'T>
    event: 'TEvent option }
  
  member this.SendEvent = this.result.IsSuccess && this.event.IsSome

module DomainOutput =
  let from o ev = { result = o; event = ev } 