module Store.Infrastructure.UserVerificationData
(*
open System
open System.Data
open Store.Domain.Health
open Store.Domain.Health.Interfaces
open Donald
open Microsoft.Extensions.Logging

type UserVerificationData =
  { id: int
    userId: int
    expiresIn: DateTime
    isUsed: bool
    verificationCode: string }

module internal Data =
  open Donald

  let toUserVerification (uvd: UserVerificationData) : UserVerification =
    { id = uvd.id
      userId = uvd.userId
      hash = None
      user = None
      expiresIn = uvd.expiresIn
      isUsed = uvd.isUsed
      verificationCode = uvd.verificationCode }

  let ofUserVerification (uv: UserVerification) : UserVerificationData =
    { id = uv.id
      userId = uv.userId
      isUsed = uv.isUsed
      expiresIn = uv.expiresIn
      verificationCode = uv.verificationCode }

  let ofDataReader (rd: IDataReader) : UserVerificationData =
    { id = rd.ReadInt32 "id"
      userId = rd.ReadInt32 "userId"
      isUsed = rd.ReadBoolean "isUsed"
      expiresIn = rd.ReadDateTime "expiresIn"
      verificationCode = rd.ReadString "verificationCode" }

module internal Queries =
  open Donald

  let insertUserVerification (db: IDbConnection) (uv: UserVerification) =
    db
    |> Db.newCommand "INSERT INTO UserVerifications (userId, expiresIn, verificationCode, isUsed) VALUES (@userId, @expiresIn, @verificationCode, @isUsed)"
    |> Db.setParams
      [ "userId", SqlType.Int32 uv.userId
        "expiresIn", SqlType.DateTime uv.expiresIn
        "isUsed", SqlType.Boolean uv.isUsed
        "verificationCode", SqlType.String uv.verificationCode ]
    |> Db.Async.exec

  let updateUserVerification (db: IDbConnection) (uv: UserVerification) =
    db
    |> Db.newCommand "UPDATE UserVerifications SET userId = @userId, expiresIn = @expiresIn, verificationCode = @verificationCode, isUsed = @isUsed WHERE id = @id"
    |> Db.setParams
      [ "id", SqlType.Int32 uv.id
        "userId", SqlType.Int32 uv.userId
        "isUsed", SqlType.Boolean uv.isUsed
        "expiresIn", SqlType.DateTime uv.expiresIn
        "verificationCode", SqlType.String uv.verificationCode ]
    |> Db.Async.exec

  let getManyByCode (db: IDbConnection) (code: string) =
    db
    |> Db.newCommand "SELECT * FROM UserVerifications WHERE verificationCode = @code"
    |> Db.setParams [ "code", SqlType.String code ]
    |> Db.Async.query Data.ofDataReader
    
    
  let getByUserEmail (db: IDbConnection) (email: string) =
    db
    |> Db.newCommand "SELECT uv.* FROM UserVerifications uv inner join Users u on u.id = uv.userId WHERE u.email = @email"
    |> Db.setParams [ "email", SqlType.String email ]
    |> Db.Async.query Data.ofDataReader

module internal Effects =
  let insertOne (dbx: IDbContext) (uv: UserVerification) =
    async {
      try
        do! Queries.insertUserVerification dbx.connection uv |> Async.AwaitTask
        return succeed uv
      with ex ->
        dbx.logger.LogError(ex, "Error inserting user verification")
        return internalError "Error saving user verification"
    }

  let updateOne (dbx: IDbContext) (uv: UserVerification) =
    async {
      try
        do! Queries.updateUserVerification dbx.connection uv |> Async.AwaitTask
        return succeed uv
      with ex ->
        dbx.logger.LogError(ex, "Error updating user verification")
        return internalError "Error updating user verification"
    }

  let readManyByCode (dbx: IDbContext) (code: string) =
    async {
      try
        let! items = Queries.getManyByCode dbx.connection code |> Async.AwaitTask
        return items |> List.map Data.toUserVerification |> succeed
      with ex ->
        dbx.logger.LogError(ex, "Error reading user verifications by code")
        return internalError "Error reading user verifications"
    }
    
  let getByUserEmail (dbx: IDbContext) (email: string) =
    async {
      try
        let! items = Queries.getByUserEmail dbx.connection email |> Async.AwaitTask
        return items |> List.map Data.toUserVerification |> succeed
      with ex ->
        dbx.logger.LogError(ex, "Error reading user verifications by code")
        return internalError "Error reading user verifications"
    }

// Public functions
let getByUserEmail dbx = Effects.getByUserEmail dbx
let insertUserVerification dbx = Effects.insertOne dbx
let updateUserVerification dbx = Effects.updateOne dbx
let getUserVerificationsByCode dbx = Effects.readManyByCode dbx
*)
