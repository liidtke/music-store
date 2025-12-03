[<AutoOpen>]
module Store.Context

open System
open System.Data
open System.Security.Claims
open Falco
open Store.Domain
open Store.Domain.Email
open Store.Domain.Health
open Store.Domain.Interfaces
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

let private getUser (logger: ILogger) (claims: Claim list) (sessionManager: SessionManager) (ctx: HttpContext) =
  let headers =
    dict [ for h in ctx.Request.Headers -> h.Key, h.Value.FirstOrDefault() ]
  // let claims = Seq.toList ctx.User.Claims
  let userId = if claims.Length > 0 then claims[0].Value else ""
  let db = ctx.Plug<IUserRepository>()
  let get: UserService.Effects.GetUser = db.Find

  let loadFrom () =
    async {
      logger.LogInformation("Loading user session: " + userId.ToString())
      let! userResult = get userId

      match userResult with
      | Success user -> return sessionManager.AddSession(user) |> succeed
      | _ -> return validation "Não foi possível buscar o usuário"
    }

  let guest () =
    { userId =
        if headers.ContainsKey("X-Guest-Id") then
          headers["X-Guest-Id"]
        else
          ""
      role = Role.Anon
      name = ""
      email = "" }

  async {
    if String.IsNullOrEmpty userId then
      return guest ()
    else
      let! session =
        match sessionManager.GetSession(userId) with
        | Some s -> async { return s |> succeed }
        | None -> loadFrom ()

      match session with
      | Success s -> return s
      | Failure f -> return guest ()


  }

let getServiceContext isAuthRequired (ctx: HttpContext) =
  let secSet = ctx.Plug<SecuritySettings>()
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
    if not authenticateResult.Succeeded && isAuthRequired then
      return validation "Invalid token"
    else
      let! session = getUser logger claims sessionManager ctx

      return
        succeed
          { userSession = session
            securitySettings = secSet
            messageBus = messageBus
            sessionManager = sessionManager
            context = ctx
            logger = logger
            emailSettings = eSet }
  }
