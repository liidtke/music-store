module Store.Infrastructure.Access.UserRepository

open System
open System.Linq
open Microsoft.Extensions.Logging
open MongoDB.Bson
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
      mapInsert logger UserMapping.toUserData UserMapping.toUser "Error adding user" collection.InsertOneAsync user

    member this.Update(user) =
      let filter =
        MongoDB.Driver.Builders<UserData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(user.id))

      let f a b = collection.ReplaceOneAsync(a, b)
      mapUpdate logger UserMapping.toUserData "Error adding user" f filter user

    member this.Fetch(p) =
      let query = collection.AsQueryable()

      let addName =
        if String.IsNullOrEmpty(p.name) then
          query
        else
          query.Where(fun u -> u.name.ToLower().Contains(p.name.ToLower()))

      let query = addName

      let query =
        if p.role.IsSome then
          query.Where(fun u -> u.role = (RoleMapping.toInt p.role.Value))
        else
          query

      let effect () = query.ToListAsync()
      mapFetch logger UserMapping.toUser "Error fetching users" effect

    member this.FetchByEmail(email) =
      let query = collection.AsQueryable().Where(fun u -> u.email = email)

      let effect () = query.FirstOrDefaultAsync()
      mapFetchSingle logger UserMapping.toUser "Error fetching user by email" effect

    member this.Find(userId) =
      let filter =
        MongoDB.Driver.Builders<UserData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(userId))

      let effect () =
        collection.Find(filter).FirstOrDefaultAsync()

      mapFind logger UserMapping.toUser "Error finding user" effect

    member this.Count(email) =
      async {
        try
          let! count =
            collection.AsQueryable().Where(fun u -> u.email = email).CountAsync()
            |> Async.AwaitTask

          return count |> succeed
        with ex ->
          return ex |> fromException

      }
