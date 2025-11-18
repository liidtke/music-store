module Store.Infrastructure.Access.UserRepository

open System.Linq
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open MongoDB.Driver
open MongoDB.Driver.Linq
open Store.Domain.Interfaces
open Store.Infrastructure.Access.Models
open Store.Infrastructure.Context


type UserRepository(ctx: IMongoContext, logger: ILogger<UserRepository>) =
  let name = "store_users"
  let collection = ctx.Collection<UserData>(name)

  interface IUserRepository with
    member this.Add(user) =
      mapInsert logger UserMapping.toUserData "Error adding user" collection.InsertOneAsync user

    member this.Update(user) =
      let filter = MongoDB.Driver.Builders<UserData>.Filter.Eq((fun p -> p.id), user.id)
      let f a b = collection.ReplaceOneAsync(a, b)
      mapUpdate logger UserMapping.toUserData "Error adding user" f filter user

    member this.Fetch(p) =
      let query =
        collection.AsQueryable().Where(fun u -> u.name.ToLower().Contains(p.name.ToLower()))

      let effect () = query.ToListAsync()
      mapFetch logger UserMapping.toUser "Error fetching users" effect

    member this.FetchByEmail(email) =
      let query = collection.AsQueryable().Where(fun u -> u.email = email)

      let effect () = query.FirstOrDefaultAsync()
      mapFetchSingle logger UserMapping.toUser "Error fetching user by email" effect

    member this.Find(userId) =
      let filter = MongoDB.Driver.Builders<UserData>.Filter.Eq((fun p -> p.id), userId)

      let effect () =
        collection.Find(filter).FirstOrDefaultAsync()

      mapFind logger UserMapping.toUser "Error finding user" effect
