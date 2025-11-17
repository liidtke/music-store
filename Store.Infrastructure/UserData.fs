module Store.Infrastructure.UserData
(*
open System.Data
open Donald
open Store.Domain.Health
open Store.Domain.Health.Access
open Store.Domain.Health.Interfaces
open Store.Domain.Health.UserService.Effects
open Store.Infrastructure.CustomRoleData
open Store.Infrastructure.DataContext
open Microsoft.Extensions.Logging
open UserService

module internal Role =
  let toInt (role: Role) =
    match role with
    | AdminRole -> 0
    | OwnerRole -> 1
    | BasicRole -> 2
    | CustomRole customRole -> 3
    | Anon -> 9

  let toCustomRoleId (role: Role) =
    match role with
    | CustomRole customRole -> customRole.id |> Some
    | _ -> None

  let toRole (value: int) (c: CustomRole option) =
    match c with
    | Some cr -> Role.CustomRole cr
    | None ->
      match value with
      | 0 -> Role.AdminRole
      | 1 -> Role.OwnerRole
      | 2 -> Role.BasicRole
      | 9 -> Role.Anon
      | _ -> Role.BasicRole

module internal Subscription =
  let toInt (sub: Subscription) =
    match sub with
    | Free -> 0
    | Basic -> 1
    | Advanced -> 2
    | Surrogate -> 3

  let toSub (value: int) =
    match value with
    | 0 -> Subscription.Free
    | 1 -> Subscription.Basic
    | 2 -> Subscription.Advanced
    | 3 -> Subscription.Surrogate
    | _ -> Subscription.Free

type UserData =
  { id: int
    name: string
    email: string
    password: string option
    role: int
    subscription: int
    customRoleId: int option
    customRole: CustomRole option
    isDeleted: bool
    isVerified: bool }

module internal UserData =

  let customRoleOfDataReader (rd: IDataReader) : CustomRole =
    { id = rd.ReadInt32 "cr_id"
      name = rd.ReadString "cr_name"
      isDeleted = rd.ReadBoolean "cr_isDeleted"
      organizationId = rd.ReadInt32 "cr_organizationId"
      permissions = rd.ReadString "cr_permissions" |> PermissionSerialization.deserialize }

  let toUser (ud: UserData) : User =
    { id = ud.id
      name = ud.name
      email = ud.email
      password = ud.password
      role = Role.toRole ud.role ud.customRole
      subscription = Subscription.toSub ud.subscription
      isDeleted = ud.isDeleted
      isVerified = ud.isVerified }

  let ofDataReader (rd: IDataReader) : UserData =
    let crid = rd.ReadInt32Option "customRoleId"

    { id = rd.ReadInt32 "id"
      name = rd.ReadString "name"
      email = rd.ReadString "email"
      password = rd.ReadStringOption "password"
      customRole =
        if crid.IsSome then
          customRoleOfDataReader rd |> Some
        else
          None
      customRoleId = crid
      role = rd.ReadInt32 "role"
      subscription = rd.ReadInt32 "subscription"
      isDeleted = rd.ReadBoolean "isDeleted"
      isVerified = rd.ReadBoolean "isVerified" }

module internal Queries =
  open Donald

  let insertUser (db: IDbConnection) (ud: User) =
    let cmd =
      "INSERT INTO Users (name, email, password, role, subscription, isDeleted, isVerified) VALUES (@name, @email, @password, @role, @isDeleted, @isVerified, @subscription)"

    db
    |> Db.newCommand cmd
    |> Db.setParams
      [ "name", SqlType.String ud.name
        "email", SqlType.String ud.email
        "password", SqlType.sqlStringOrNull ud.password
        "role", SqlType.Int32(Role.toInt ud.role)
        "subscription", SqlType.Int32(Subscription.toInt ud.subscription)
        "isDeleted", SqlType.Boolean ud.isDeleted
        "isVerified", SqlType.Boolean ud.isVerified ]
    |> Db.Async.exec

  let updatePassword (db: IDbConnection) (uid: int) (password: string) =
    db
    |> Db.newCommand "UPDATE Users set password = @password where id = @uid"
    |> Db.setParams [ "password", SqlType.String password; "uid", SqlType.Int32 uid ]
    |> Db.Async.exec

  let updateSubscription (db: IDbConnection) (uid: int) (subscriptionId: int) =
    db
    |> Db.newCommand "UPDATE Users set subscription = @subscription where id = @uid"
    |> Db.setParams [ "subscription", SqlType.Int32 subscriptionId; "uid", SqlType.Int32 uid ]
    |> Db.Async.exec

  let update (db: IDbConnection) (user: User) =
    db
    |> Db.newCommand
      "UPDATE Users set name = @name, role = @role, subscription = @subscription, customRoleId = @customRoleId where id = @uid"
    |> Db.setParams
      [ "name", SqlType.String user.name
        "role", SqlType.Int32(user.role |> Role.toInt)
        "customRoleId", SqlType.sqlInt32OrNull (user.role |> Role.toCustomRoleId)
        "subscription", SqlType.Int32(user.subscription |> Subscription.toInt)
        "uid", SqlType.Int32 user.id ]
    |> Db.Async.exec

  let countEmail (db: IDbConnection) (email: string) =
    db
    |> Db.newCommand "SELECT count(email) c from Users where email = @email"
    |> Db.setParams [ "email", SqlType.String email ]
    |> Db.Async.querySingle (fun r -> r.ReadInt32 "c")

  let getById (db: IDbConnection) (uid: int) =
    let cmd =
      """
SELECT u.*, 
       cr.id as cr_id, cr.id, cr.name as cr_name, cr.isDeleted as cr_isDeleted, cr.organizationId as cr_organizationId,  cr.permissions as cr_permissions
       from Users u 
         LEFT JOIN CustomRoles cr on u.customRoleId = cr.id
         where u.id = @id

"""
    db
    |> Db.newCommand cmd
    |> Db.setParams [ "id", SqlType.Int32 uid ]
    |> Db.Async.querySingle UserData.ofDataReader

  //with donald
  let getByEmail (db: IDbConnection) (email: string) =
    let cmd =
      """
SELECT u.*, 
       cr.id as cr_id, cr.id, cr.name as cr_name, cr.organizationId as cr_organizationId,  cr.permissions as cr_permissions, cr.isDeleted as cr_isDeleted
       from Users u 
         LEFT JOIN CustomRoles cr on u.customRoleId = cr.id
         WHERE u.email = @email
"""
    db
    |> Db.newCommand cmd
    |> Db.setParams [ "email", SqlType.String email ]
    |> Db.Async.querySingle UserData.ofDataReader

  let getByOrg (db: IDbConnection) (orgId: int) =
    let cmd =
      """
SELECT distinct u.* ,
       cr.id as cr_id, cr.id, cr.name as cr_name, cr.organizationId as cr_organizationId,  cr.permissions as cr_permissions, cr.isDeleted as cr_isDeleted
from Users u 
    LEFT JOIN CustomRoles cr on u.customRoleId = cr.id
    INNER JOIN UserOrganizations o on o.userId = u.Id where o.organizationId = @orgId

"""
    db
    |> Db.newCommand cmd
    |> Db.setParams [ "orgId", SqlType.Int32 orgId ]
    |> Db.Async.query UserData.ofDataReader

  let searchUser (db: IDbConnection) (search: string) =
    let cmd =
      """
SELECT u.*, 
       cr.id as cr_id, cr.id, cr.name as cr_name, cr.organizationId as cr_organizationId,  cr.permissions as cr_permissions, cr.isDeleted as cr_isDeleted
       from Users u 
       LEFT JOIN CustomRoles cr on u.customRoleId = cr.id
       WHERE u.name like @search or email like CONCAT('%',@search,'%') or @search = '' or @search is null
"""
    db
    |> Db.newCommand cmd
    |> Db.setParams [ "search", queryString search ]
    |> Db.Async.query UserData.ofDataReader


module internal Effects =
  let save (dbx: IDbContext) (u: User) =
    async {
      try
        let func = if u.id = 0 then Queries.insertUser else Queries.update
        do! func dbx.connection u |> Async.AwaitTask
        return succeed u
      with ex ->
        dbx.logger.LogError(ex, "Error saving Person")
        return internalError "Error saving"
    }

  let count (dbx: IDbContext) email =
    async {
      try
        let! response = Queries.countEmail dbx.connection email |> Async.AwaitTask

        match response with
        | None -> return internalError "Error"
        | Some value -> return succeed value

      with ex ->
        return internalError "Error processing"
    }

  let getByEmail (dbx: IDbContext) email =
    async {
      try
        let! response = Queries.getByEmail dbx.connection email |> Async.AwaitTask

        match response with
        | None -> return invalidate "Not Found"
        | Some value -> return value |> UserData.toUser |> succeed
      with ex ->
        dbx.logger.LogError(ex, "Error searching user")
        return invalidate "Not Found"
    }

  let getUser (dbx: IDbContext) (uid: int) =
    async {
      try
        let! response = Queries.getById dbx.connection uid |> Async.AwaitTask

        match response with
        | None -> return invalidate "Not Found"
        | Some value -> return value |> UserData.toUser |> succeed
      with ex ->
        return invalidate "Not Found"
    }

  let searchUser (dbx: IDbContext) (input: string) =
    //no futuro filtrar por role / org admin só podem ver admin
    let search = if input = null then "" else input.ToLower()

    async {
      try
        let! items = Queries.searchUser dbx.connection search |> Async.AwaitTask
        return items |> List.map (fun i -> i |> UserData.toUser) |> succeed
      with ex ->
        dbx.logger.LogWarning(ex, "Erro ao buscar")
        return invalidate "Not Found"
    }

//start public

let getByOrgId (dbx: IDbContext) orgId =
  async {
    let! result = Queries.getByOrg dbx.connection orgId |> tryAsync dbx.logger "Erro ao buscar"

    match result with
    | Success uds -> return uds |> List.map UserData.toUser |> succeed
    | Failure errorResult -> return internalError "Erro ao buscar"
  }

let factory (dbx: IDbContext) : Dependencies =
  { save = Effects.save dbx
    count = Effects.count dbx
    get = Effects.getUser dbx }

let save = Effects.save
let searchUser dbx = Effects.searchUser dbx
let getUserByEmail dbx : UserService.Effects.GetUserByEmail = Effects.getByEmail dbx
let getUser dbx : UserService.Effects.GetUser = Effects.getUser dbx

type UserRepository(dbx: IDbContext) =
  interface IUserRepository with
    member this.GetUser = fun uid -> Effects.getUser dbx uid
    member this.Save(user) = Effects.save dbx user
*)
