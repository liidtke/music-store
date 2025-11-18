module Store.Infrastructure.Access.UserVerificationRepository

open System.Linq
open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.Driver.Linq
open Store.Domain.Interfaces
open Store.Infrastructure.Access.Models
open Store.Infrastructure.Context

type UserVerificationRepository(ctx: IMongoContext, logger: ILogger<UserVerificationRepository>) =
  let verificationsCollectionName = "store_user_verifications"
  let usersCollectionName = "store_users"

  let verifications =
    ctx.Collection<UserVerificationData>(verificationsCollectionName)

  // let users = ctx.Collection<UserData>(usersCollectionName)

  interface IUserVerificationRepository with
    member this.Add(verification) =
      mapInsert
        logger
        UserVerificationMapping.toUserVerificationData
        (UserVerificationMapping.toUserVerificationUserOption verification.user)
        "Error adding user verification"
        verifications.InsertOneAsync
        verification

    member this.Update(verification) =
      let filter =
        Builders<UserVerificationData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(verification.id))

      let f a b = verifications.ReplaceOneAsync(a, b)

      mapUpdate
        logger
        UserVerificationMapping.toUserVerificationData
        "Error updating user verification"
        f
        filter
        verification

    member this.Find(id) =
      let filter =
        Builders<UserVerificationData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(id))

      let effect () =
        verifications.Find(filter).FirstOrDefaultAsync()

      mapFind logger UserVerificationMapping.toUserVerification "Error finding user verification" effect

    member this.FetchByUserEmail(email) =
      // Busca o usuário relacionado a um determinado userId
      async {
        let! user =
          ctx.Collection<UserData>(usersCollectionName).Find(fun u -> u.email = email).FirstOrDefaultAsync()
          |> Async.AwaitTask

        if isNull (box user) then
          return notFound "Usuário não encontrado"
        else
          let query () =
            verifications.AsQueryable().Where(fun v -> v.userId = user.id && v.isUsed = false).ToListAsync()

          return!
            mapFetch
              logger
              (UserVerificationMapping.toUserVerificationUser user)
              "Error fetching user from verification"
              query
      }
