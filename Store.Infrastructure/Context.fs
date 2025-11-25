module Store.Infrastructure.Context

open System.Linq
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver

type DatabaseSettings =
  { connectionString: string
    databaseName: string }

type IMongoContext =
  abstract member Collection<'a> : string -> IMongoCollection<'a>
  abstract member Query<'a> : string -> IQueryable<'a>

type MongoContext(settings: DatabaseSettings) =
  let client: MongoClient = MongoClient(settings.connectionString)
  let database: IMongoDatabase = client.GetDatabase(settings.databaseName)

  interface IMongoContext with
    member this.Collection<'a> name = database.GetCollection<'a>(name)

    member this.Query<'a> name =
      database.GetCollection<'a>(name).AsQueryable() :> IQueryable<'a>

let mapInsert
  (logger: ILogger)
  (mapperIn: 'entity -> 'data)
  (mapperOut: 'data -> 'entity)
  (message: string)
  (effect: 'data -> Task)
  (input: 'entity)
  =
  async {
    try
      let data = input |> mapperIn
      do! data |> effect |> Async.AwaitTask
      return mapperOut data |> succeed 
    with ex ->
      logger.LogError(ex, message) |> ignore
      if ex.InnerException <> null && ex.InnerException.Message.ToLower().Contains("duplicate") then return fail "Duplicate Item"
        else return fail message
  }

let mapUpdate
  (logger: ILogger)
  (mapperIn: 'entity -> 'data)
  // (mapperOut: 'data -> 'entity)
  (message: string)
  (effect: FilterDefinition<'data> -> 'data -> Task<ReplaceOneResult>)
  (filter: FilterDefinition<'data>)
  (input: 'entity)
  =
  async {
    try
      let data = mapperIn input
      let! _ = effect filter data |> Async.AwaitTask
      //later check result.IsAcknowledged
      // if result.ModifiedCount = 0 then
      //   return fail message
      // else
      return succeed input
    with ex ->
      logger.LogError(ex, message) |> ignore
      return fail message
  }

let mapFetch
  (logger: ILogger)
  (mapperOut: 'data -> 'entity)
  (message: string)
  (effect: unit -> Task<System.Collections.Generic.List<'data>>)
  =
  async {
    try
      let! result = effect () |> Async.AwaitTask
      return result |> Seq.map mapperOut |> Seq.toList |> succeed
    with ex ->
      logger.LogError(ex, message) |> ignore
      return fail message
  }

let mapFetchSingle
  (logger: ILogger)
  (mapperOut: 'data -> 'entity)
  (errorMessage: string)
  (effect: unit -> Task<'data>)
  =
  async {
    try
      let! result = effect () |> Async.AwaitTask

      if isNull (box result) then
        return notFound "Not Found"
      else
        return result |> mapperOut |> succeed
    with ex ->
      logger.LogError(ex, errorMessage) |> ignore
      return fail errorMessage
  }

let mapFind (logger: ILogger) (mapperOut: 'data -> 'entity) (errorMessage: string) (effect: unit -> Task<'data>) =
  async {
    try
      let! result = effect () |> Async.AwaitTask

      if isNull (box result) then
        return notFound "Not Found"
      else
        return result |> mapperOut |> succeed
    with ex ->
      logger.LogError(ex, errorMessage) |> ignore
      return fail errorMessage
  }

let parseExec<'t> (id: string) (effect: BsonObjectId -> Async<ServiceOutput<'t>>) =
  match ObjectId.TryParse id with
  | true, objectId -> effect objectId
  | false, objectId -> async {return validation<'t> "Invalid id"}

