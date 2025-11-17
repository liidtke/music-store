module Hs.Pages.Login

open System
open System.Data
open System.Threading.Tasks
open Application
open Falco
open Falco.Routing
open Falco.Markup
open Hs
open Hs.Components
open Hs.Domain
open Hs.Domain.Access
open Hs.Domain.Interfaces
open Hs.Infrastructure
open Hs.Infrastructure.DataContext
open Hs.Infrastructure.Integrations
open Hs.Pages.Users.Details
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
//refactor
//https://www.patternfly.org/components/login-page/react/basic/

module Login2 =
  open Falco.Markup
  
  
  // SVG Icons
  let userIcon = 
      Elem.svg [ _xmlns "http://www.w3.org/2000/svg"; _fill "#bbb"; _stroke "#bbb"; _class "w-4 h-4 absolute right-4"; _viewBox "0 0 24 24" ] [
          _circle [ _cx "10"; _cy "7"; _r "6";  ] []
          _path [ _d "M14 15H6a5 5 0 0 0-5 5 3 3 0 0 0 3 3h12a3 3 0 0 0 3-3 5 5 0 0 0-5-5zm8-4h-2.59l.3-.29a1 1 0 0 0-1.42-1.42l-2 2a1 1 0 0 0 0 1.42l2 2a1 1 0 0 0 1.42 0 1 1 0 0 0 0-1.42l-.3-.29H22a1 1 0 0 0 0-2z";  ] []
      ]
  
  let eyeIcon = 
      Elem.svg [ _xmlns "http://www.w3.org/2000/svg"; _fill "#bbb"; _stroke "#bbb"; _class "w-4 h-4 absolute right-4 cursor-pointer"; _viewBox "0 0 128 128" ] [
          _path [ _d "M64 104C22.127 104 1.367 67.496.504 65.943a4 4 0 0 1 0-3.887C1.367 60.504 22.127 24 64 24s62.633 36.504 63.496 38.057a4 4 0 0 1 0 3.887C126.633 67.496 105.873 104 64 104zM8.707 63.994C13.465 71.205 32.146 96 64 96c31.955 0 50.553-24.775 55.293-31.994C114.535 56.795 95.854 32 64 32 32.045 32 13.447 56.775 8.707 63.994zM64 88c-13.234 0-24-10.766-24-24s10.766-24 24-24 24 10.766 24 24-10.766 24-24 24zm0-40c-8.822 0-16 7.178-16 16s7.178 16 16 16 16-7.178 16-16-7.178-16-16-16z"; ] []
      ]
  
  // Form Fields
  let emailField = 
      Elem.div [] [
          Elem.label [ _class "text-slate-900 text-sm font-medium mb-2 block" ] [ Text.raw "Email" ]
          Elem.div [ _class "relative flex items-center" ] [
              Elem.input [ Attr.name "username"; Attr.type' "text"; Attr.required; _class "w-full text-slate-900 text-sm border border-slate-300 px-4 py-3 pr-8 rounded-md"; Attr.placeholder "usuario@usuario.com" ] 
              userIcon
          ]
      ]
  
  let passwordField = 
      Elem.div [] [
          Elem.label [ _class "text-slate-900 text-sm font-medium mb-2 block" ] [ Text.raw "Senha" ]
          Elem.div [ _class "relative flex items-center" ] [
              Elem.input [ Attr.name "password"; Attr.type' "password"; Attr.required; _class "w-full text-slate-900 text-sm border border-slate-300 px-4 py-3 pr-8 rounded-md"; Attr.placeholder "Insira sua senha" ]
              eyeIcon
          ]
      ]
  
  let rememberMeSection = 
      Elem.div [ _class "flex flex-wrap items-center justify-between gap-4" ] [
          Elem.div [ _class "flex items-center" ] [
              Elem.input [ Attr.id "remember-me"; Attr.name "remember-me"; Attr.type' "checkbox"; _class "h-4 w-4 shrink-0 text-primary focus:ring-blue-500 border-slate-300 rounded" ] 
              Elem.label [ Attr.for' "remember-me"; _class "ml-3 block text-sm text-slate-900" ] [ Text.raw "Lembrar de mim" ]
          ]
          Elem.div [ _class "text-sm" ] [
              Elem.a [ Attr.href "/#"; _class "text-primary hover:underline font-semibold" ] [ Text.raw "Esqueceu sua senha?" ]
          ]
      ]
  
  let submitButton = 
      Elem.div [ _class "!mt-12" ] [
          Elem.button [ Attr.type' "button"; _class "w-full py-2 px-4 text-[15px] font-medium tracking-wide rounded-md text-white bg-primary focus:outline-none cursor-pointer" ] [ Text.raw "Entrar" ]
      ]
  
  let signupLink = 
      Elem.p [ _class "text-slate-900 text-sm !mt-6 text-center" ] [
          Text.raw "Não possui uma conta? "
          Elem.a [ _class "text-primary hover:underline ml-1 whitespace-nowrap font-semibold" ] [ Text.raw "Cadastrar" ]
      ]
  
  // Main Form
  let loginForm = 
      Elem.form [ _class "mt-12 space-y-6" ] [
          emailField
          passwordField
          rememberMeSection
          submitButton
          signupLink
      ]
  
  // Main Layout
  let main = 
      Elem.div [ _class "bg-gray-50" ] [
          Elem.div [ _class "min-h-screen flex flex-col items-center justify-center py-6 px-4" ] [
              Elem.div [ _class "max-w-[480px] w-full" ] [
                  Elem.a [ Attr.href "#" ] [ (* logo placeholder *) ]
                  Elem.div [ _class "p-6 sm:p-8 rounded-2xl border border-gray-200 shadow-sm" ] [
                      Elem.h1 [ _class "text-slate-900 text-center text-3xl font-semibold" ] [ Text.raw "Entrar" ]
                      loginForm
                  ]
              ]
          ]
      ]

module SelectOrg =
  let content (orgs:Organization list) =
    [
      Web.Section.main []
      Web.Modal.basic true "sm" "Teste" [] []
    ]

module Wrapper =
  open Falco.Markup.Elem


  let content m =
    div
      [ _class "pf-v6-c-login" ]
      [ div
          [ _class "pf-v6-c-login__container" ]
          [ header
              [ _class "pf-v6-c-login__header" ]
              [ img
                  [ _class "pf-v6-c-brand"
                    Attr.alt "Logo"
                    Attr.src "/static/tst.png"
                    _style "--pf-v6-c-brand--Height:48px;" ] ]
            main [ _class "pf-v6-c-login__main" ] m
            footer [ _class "pf-v6-c-login__footer" ] [ p [] [ _text "Novo sistema" ] ] ] ]

  let leftPane showWarning m =
    [ header
        [ _class "pf-v6-c-login__main-header" ]
        [ h1 [ _class "pf-v6-c-title pf-m-3xl" ] [ _text "Realizar Login" ]
          p [ _class "pf-v6-c-login__main-header-desc" ] [ _text "Esse é o subtexto do login" ] ]
      div
        [ _class "pf-v6-c-login__main-body" ]
        [ div
            [ _class "pf-v6-c-form" ]
            [ Elem.button
                [ _id "login-auth"
                  // Attr.onclick "login()"
                  _hypr "on click show #kk6 then call login()"
                  _class "pf-v6-c-button pf-m-block pf-m-primary pf-m-in-progress pf-m-progress" ]
                [
                  Web.Button.spinner "kk6"
                  span [_class "pf-v6-c-button__text"] [_text "Efetuar Login"]
                   ]
              if showWarning then
                [Elem.div [ _class "p-form-validation__message" ] [ Web.Icons.alert ;_text "Usuário não localizado ou senha incorreta" ]] |> Web.Panel.raised
              else
                () ]
          m ]
      footer
        [ _class "pf-v6-c-login__main-footer" ]
        [

          div
            [ _class "pf-v6-c-login__main-footer-band" ]
            [ p [ _class "pf-v6-c-login__main-footer-band-item" ] [ a [ _hypr "on click show #form-2" ] [ _text "Já possui uma conta?" ] ]
              p [ _class "pf-v6-c-login__main-footer-band-item" ] [ _text ""; a [ _href "/forgot-password" ] [ _text "Recuperar Senha" ] ] ]

          ] ]
//https://www.patternfly.org/components/login-page/html/basic/
let main showWarning =
  let f =
    Elem.form
      [ _id "form-2"
        _class "pf-v6-c-form"
        _post "/login"
        _target "body"
        _style "max-width: 600px; display:none;" ]
      [ Elem.div [ _class "pf-v6-c-form__helper-text" ] [ _text "Caso já possua um usuário e senha você pode logar aqui" ]
        Elem.div
          [ _class "pf-v6-c-form__group" ]
          [ Elem.label [ _for "username"; _class "pf-v6-c-form__label" ] [ Elem.span [ _class "pf-v6-c-form__label-text" ] [ _text "Email" ] ]
            Elem.span [ _class "pf-v6-c-form-control pf-m-required" ] [ Elem.input [ _type "email"; _id "email"; _name "email"; Attr.placeholder "email@site.com" ] ]

            ]

        Elem.div
          [ _class "pf-v6-c-form__group" ]
          [ Elem.label [ _for "password"; _class "pf-v6-c-form__label" ] [ Elem.span [ _class "pf-v6-c-form__label-text" ] [ _text "Senha" ] ]
            Elem.span [ _class "pf-v6-c-form-control pf-m-required" ] [ Elem.input [ _type "password"; _id "password"; _name "pwd" ] ]

            ]
        Elem.div [ _class "pf-v6-c-form__group pf-m-action" ] [ Elem.button [ _class "pf-v6-c-button pf-m-block pf-m-primary"; _type "submit" ] [ _text "Login" ] ] ]


  [ f |> Wrapper.leftPane showWarning |> Wrapper.content ] |> Html.mainHtml "Login" ""


let homeHandler (ctx: HttpContext) =
  if ctx.User.Identity.IsAuthenticated then
    Response.redirectTemporarily "/" ctx
  else
    let home = [Login2.main] |> Html.mainHtml "Login | HS System" "light" 
    Response.ofHtml home ctx

let home = get "/login" homeHandler

let signOut =
  get "/logout" (Response.signOutAndRedirect Security.cookieScheme "/login")

module Register =
  type RegisterUser =
    { name: string
      email: string
      password: string }

module SignInPwd =

  type LoginRequest = { email: string; password: string }

  let workflow (a: LoginRequest) =
    fun (ctx: HttpContext) ->
      let getUserEffect =
        ctx.Plug<IDbConnection>() |> DbContext.anon |> UserData.getUserByEmail

      let config: Security.SecuritySettings =
        ctx.RequestServices.GetService<Security.SecuritySettings>()

      let userOutput = getUserEffect a.email |> Async.RunSynchronously
      
      let getOrg (user:User) =
        None
        // let effect = ctx.Plug<IDbConnection>() |> DbContext.anon |> OrganizationData.getByUserId
        // let output = effect user.id |> Async.RunSynchronously
        // match output with
        // | Success organization -> Some(organization)
        // | Failure errorResult -> None

      let ret u =
        if u |> Security.validateUserPassword config a.password then
          Response.signInAndRedirect Security.cookieScheme (Security.userToPrincipal u <| getOrg u) "/" ctx
        else
          Response.ofHtml (main true) ctx

      match userOutput with
      | Success user -> ret user
      | Failure errorResult -> Response.ofHtml (main true) ctx

  let formHandler: HttpHandler =
    Request.mapForm
      (fun f ->
        let email = f.GetString("email")
        let pwd = f.GetString("pwd")
        { email = email; password = pwd }: LoginRequest)
      workflow

module SignInAuth0 =

  let getOrCreate (ctx: HttpContext) (up: UserProfile) =
    async {
      let getUserEffect =
        ctx.Plug<IDbConnection>() |> DbContext.anon |> UserData.getUserByEmail

      let userEffects = ctx.Plug<IDbConnection>() |> DbContext.anon |> UserData.factory
      let! existingOutput = getUserEffect up.email

      let createService =
        UserService.Create.handle UserSession.anon (fun i -> i) userEffects

      let create () =
        async {
          let input =
            { name = up.name
              email = up.email
              role = "UserRole" }
            : UserService.Create.CreateUser

          let! u = createService input

          match u with
          | Success user -> return succeed user
          | Failure errorResult -> return validation errorResult.Message
        }

      match existingOutput with
      | Success user -> return succeed user
      | Failure _ -> return! create ()
    }

  let confirmHandler: HttpHandler =
    secureWithScope
    <| fun ctx ->

      let returnCookie u =
        Response.signInAndRedirect Security.cookieScheme (Security.userToPrincipal u None) "/" ctx

      let auth =
        ctx.Request.Headers
        |> Seq.map (fun c -> (c.Key, Seq.head c.Value))
        |> dict
        |> Seq.where (fun i -> i.Key = "Authorization")
        |> Seq.tryHead

      let token =
        match auth with
        | Some v -> v.Value.Replace("Bearer ", "")
        | _ -> ""

      let userProfile = Integrations.Auth0.getProfile token |> Async.RunSynchronously

      match userProfile with
      | Success userProfile ->
        let userOutput = getOrCreate ctx userProfile |> Async.RunSynchronously

        match userOutput with
        | Success user -> returnCookie user
        | Failure errorResult -> Response.ofHtml (main true) ctx

      | Failure errorResult -> Response.ofHtml (main true) ctx

  let confirm = get "/callback-confirm" (confirmHandler)

//start front-end login process
let callbackHandler: HttpHandler =
  [ [ Elem.div [ _hypr "on load call postLogin()" ] [ _text "processing" ] ]
    |> Web.pageMain ]
  |> Html.mainHtml "Login" ""
  |> Response.ofHtml

let signIn = post "/login" SignInPwd.formHandler
let callback = get "/callback" callbackHandler

let endpoints = [ home; signIn; signOut; callback; SignInAuth0.confirm ]
