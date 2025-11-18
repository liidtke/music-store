module Store.Infrastructure.Access.UserVerificationRepository

open System.Linq
open System.Threading.Tasks
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
        UserVerificationMapping.toUserVerification
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

    member this.FetchByUser(userId) =
      // Busca o usuário relacionado a um determinado userId

      let query () =
        verifications
          .AsQueryable()
          .Where(fun v -> v.userId = BsonObjectId.Create(userId) && v.isUsed = false)
          .ToListAsync()

      mapFetch logger UserVerificationMapping.toUserVerification "Error fetching user from verification" query
