module Store.Infrastructure.Context

open System.Linq
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
        member this.Query<'a> name = database.GetCollection<'a>(name).AsQueryable() :> IQueryable<'a>
