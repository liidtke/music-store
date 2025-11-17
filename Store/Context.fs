[<AutoOpen>]
module Store.Context

open System
open System.Data
open System.Security.Claims
open Falco
open Store.Domain.Email
open Store.Domain.Health
open Store.Domain.Health.Interfaces
open Store.Infrastructure.DataContext
open Store.Security
open Store.Session
open Microsoft.AspNetCore.Authentication
open Microsoft.Extensions.Logging
open SharedKernel
open Microsoft.AspNetCore.Http
open System.Linq
open Wolverine

type ServiceContext =
  { userSession: UserSession
    securitySettings: SecuritySettings
    emailSettings: EmailSettings
    messageBus: IMessageBus
    sessionManager: SessionManager
    context: HttpContext
    logger: ILogger }

let private getUser
  (logger: ILogger)
  (claims: Claim list)
  (sessionManager: SessionManager)
  (db: IDbConnection)
  (ctx: HttpContext)
  =
  let headers =
    dict [ for h in ctx.Request.Headers -> h.Key, h.Value.FirstOrDefault() ]
  // let claims = Seq.toList ctx.User.Claims
  let userId = claims[0].Value |> int

  let get: UserService.Effects.GetUser =
    fun i ->
      async {
        return
          { id = 0
            name = "test"
            email = "test"
            password = "test" |> Some
            role = Role.CustomerRole
            isDeleted = false
            isVerified = true }
          |> succeed

      }

  let loadFrom () =
    async {
      logger.LogInformation("Loading user session: " + userId.ToString())
      let! userResult = get userId

      match userResult with
      | Success user -> return sessionManager.AddSession(user) |> succeed
      | _ -> return validation "Não foi possível buscar o usuário"
    }

  async {
    let! session =
      match sessionManager.GetSession(userId) with
      | Some s -> async { return s |> succeed }
      | None -> loadFrom ()

    match session with
    | Success s -> return s
    | Failure f ->
      return
        { userId = 0
          role = Role.Anon
          name = ""
          email = "" }

  }

let getServiceContext isAuthRequired (ctx: HttpContext) =
  let secSet = ctx.Plug<SecuritySettings>()
  let db = ctx.Plug<IDbConnection>()
  let messageBus = ctx.Plug<IMessageBus>()
  let sessionManager = ctx.Plug<SessionManager>()
  let eSet = ctx.Plug<EmailSettings>()
  let logger = ctx.Plug<ILogger<User>>()

  let authenticateResult =
    ctx.AuthenticateAsync("Bearer") |> Async.AwaitTask |> Async.RunSynchronously

  let claims =
    if authenticateResult.Succeeded then
      Seq.toList authenticateResult.Principal.Claims
    else
      []

  async {
    if authenticateResult.Succeeded then
      let! session = getUser logger claims sessionManager db ctx

      return
        succeed
          { userSession = session
            securitySettings = secSet
            messageBus = messageBus
            sessionManager = sessionManager
            context = ctx
            logger = logger
            emailSettings = eSet }
    elif not isAuthRequired then
      return
        succeed
          { userSession = UserSession.anon
            securitySettings = secSet
            context = ctx
            messageBus = messageBus
            sessionManager = sessionManager
            logger = logger
            emailSettings = eSet }
    else
      return validation "Invalid Token"
  }
