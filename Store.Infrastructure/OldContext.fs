module Store.Infrastructure.DataContext
(*

open System
open System.Data
open System.Threading.Tasks
open Donald
open Store.Domain.Health
open Store.Domain.Health.Interfaces
open Microsoft.Extensions.Logging

type DbContext(connection: IDbConnection, logger: ILogger) =

  interface IDbContext with
    member this.connection = connection
    member this.logger = logger


module DbContext =
  let anon (connection: IDbConnection) (logger: ILogger) = DbContext(connection, logger)

  let known (connection: IDbConnection) (logger: ILogger) userId = DbContext(connection, logger)

let objToInt (value: obj) : int =
  match value with
  | :? int as i -> i
  | :? string as s ->
    match System.Int32.TryParse(s) with
    | true, i -> i
    | false, _ -> 0
  | :? float as f -> (int f) // Truncates decimal part
  | :? int64 as i64 -> (int i64)
  | _ -> 0



let tryAsync<'a> (logger: ILogger) (message: string) (func: Task<'a>) =
  async {
    try
      let! result = func |> Async.AwaitTask
      return succeed result
    with ex ->
      logger.LogError(ex, message) |> ignore
      return internalError message
  }


let tryAsyncWithResult<'a> (logger: ILogger) (message: string) (ret: 'a) (func: Task<unit>) =
  async {
    try
      do! func |> Async.AwaitTask
      return succeed ret
    with ex ->
      logger.LogError(ex, message) |> ignore
      return internalError message
  }

type DataQuery<'a, 'b> = IDbContext -> 'a -> Task<'b>

let prepException (ex: Exception) (msg: string) =
  let message =
    if ex.InnerException.InnerException <> null then
      ex.InnerException.InnerException.Message
    elif ex.InnerException <> null then
      ex.InnerException.Message
    else
      ex.Message

  if message.Contains("unique") then
    "Item Duplicado"
  elif message.Contains("organizationId") then
    "Selecione uma Organização"
  else
    msg


let runEffect (dbx: IDbContext) (msg: string) (q: DataQuery<'a, 'b>) (input: 'a) =
  async {
    try
      let! result = q dbx input |> Async.AwaitTask
      return succeed result
    with ex ->
      dbx.logger.LogError(ex, msg) |> ignore
      let msg = prepException ex msg
      return fail msg
  }

let runEffectResult (dbx: IDbContext) (msg: string) (q: DataQuery<'a, 'b>) (input: 'a) =
  async {
    try
      let! result = q dbx input |> Async.AwaitTask
      return succeed input
    with ex ->
      dbx.logger.LogError(ex, msg) |> ignore
      return internalError msg
  }

let isoString (d: DateTime) =
  SqlType.String <| d.ToString("yyyy-MM-dd")

let paramList ids =
  ids |> List.mapi (fun i id -> sprintf "@id%d" i, SqlType.Int32 id)

let inClause ids =
  ids |> List.mapi (fun i _ -> sprintf "@id%d" i) |> String.concat ", "

let queryString (t: string) = "%" + t + "%" |> SqlType.String

let queryOption (t: string option) =
  match t with
  | None -> None
  | Some value -> value |> queryString |> Some

let inline hasColumn (reader: IDataReader) (columnName: string) : bool =
  try
    let _ = reader.GetOrdinal columnName
    true
  with :? IndexOutOfRangeException ->
    false

*)