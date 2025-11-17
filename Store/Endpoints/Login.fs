module Store.Endpoints.Login

open System
open Application
open Falco
open Falco.Routing
open Store
open Store.Domain
open Store.Domain.Health
open Store.Domain.Health.Interfaces
open Store.Infrastructure
open Store.Security
open Store.Session
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
(*
module RequestPasswordConfirm =
  type RequestChangePasswordInput =
    { email: string
      code: string
      password: string }

  let workflow (stx: ServiceContext) (input: RequestChangePasswordInput) =
    let db = stx.getDbContext ()
    let salt = Crypto.createSalt
    let genCode = Crypto.sha256 2 32
    let getUserEffect = UserData.getUserByEmail db
    let passwordHasher = hashPassword stx.securitySettings
    let getUvByEmail = UserVerificationData.getByUserEmail db
    let update = UserVerificationData.updateUserVerification db
    let saveUser = UserData.save db

    //send mail
    let sender =
      EmailService.sendPasswordChanged
        stx.emailSettings
        MailSender.sendEmailAsync
        EmailTemplate.renderPasswordChangedEmail

    async {
      let! confirmation =
        UserVerificationService.confirmChangePassword input.email input.code getUvByEmail update genCode

      let! result =
        match confirmation with
        | Success userVerification ->
          UserService.Update.changePassword input.email input.password saveUser getUserEffect passwordHasher
        | Failure errorResult -> async { return failure errorResult }

      let! _ = result ?>< sender
      return result ?> (fun i -> input.email |> succeed)
    }

  let handler: HttpHandler =
    runAnonAsync workflow |> Request.mapJson<RequestChangePasswordInput>

  let endpoint = post "/login/password-confirm" handler

module RequestPasswordReset =
  type RequestPasswordInput = { email: string }

  let workflow (stx: ServiceContext) (input: RequestPasswordInput) =
    let db = stx.getDbContext ()
    let salt = Crypto.createSalt
    let genCode = Crypto.sha256 2 32
    let getUserEffect = UserData.getUserByEmail db
    let saveVerificationEffect = UserVerificationData.insertUserVerification db

    //send mail
    let sender =
      EmailService.sendPasswordResetRequest
        stx.emailSettings
        MailSender.sendEmailAsync
        EmailTemplate.renderRecoverPasswordEmail

    async {
      let! result =
        UserVerificationService.requestPasswordReset input.email getUserEffect saveVerificationEffect salt genCode

      let! sendResult = result ?>< sender
      return result ?> (fun i -> succeed i.hash.Value)
    }

  let handler: HttpHandler =
    runAnonAsync workflow |> Request.mapJson<RequestPasswordInput>

  let endpoint = post "/login/password-reset" handler

*)
type LoginRequest =
  { email: string
    password: string
    rememberMe: bool }

let workflow (a: LoginRequest) : HttpHandler =
  fun (ctx: HttpContext) ->
    let sessionManager = ctx.Plug<SessionManager>()
    // let getUserEffect = UserData.getUserByEmail db

    let getUserEffect =
      fun (i:string) ->
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

    let config: Security.SecuritySettings =
      ctx.RequestServices.GetService<Security.SecuritySettings>()

    let ret (u: User) =
      let session = sessionManager.AddSession(u)
      Application.setHeaders ctx session |> ignore

      let config =
        if a.rememberMe then
          { config with
              TokenExpirationTime = 900 }
        else
          config

      if u |> Security.validateUserPassword config a.password then
        Response.ofJson <| Security.generateToken config u
      else
        Response.withStatusCode 401
        >> Response.ofPlainText "Usuário não encontrado ou senha inválida"

    task {

      let! userOutput = getUserEffect a.email

      match userOutput with
      | Success user -> return ret user ctx
      | Failure errorResult ->
        return
          (Response.withStatusCode 401
           >> Response.ofPlainText "Usuário não encontrado ou senha inválida")
            ctx
    }

let formHandler: HttpHandler = Request.mapJson<LoginRequest> workflow


let login = post "/login" formHandler

let endpoints =
  [ login
    // RequestPasswordReset.endpoint
    // RequestPasswordConfirm.endpoint
     ]
