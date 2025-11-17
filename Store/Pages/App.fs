module Hs.Pages.App

open Application
open Hs.Components.Base
open Falco.Markup
open Falco
open Falco.Routing
open Hs.Components
open Hs.Domain
open Hs.Infrastructure
open Hs.Security
open Hs.Session

let notFoundHandler: HttpHandler =
  let r1 =
    [ Web2.pageMain
        [ Web.Title.page "Não Encontrado" "Atenção"
          Web.Section.main
            [ Elem.p [ _class "u-align-text--center" ] [ _text "A página não foi encontrada ou não existe" ] ] ] ]
    |> Html.mainHtml "Não encontrado" ""
    |> Response.ofHtml

  Response.withStatusCode 404 >> r1

let notFoundEndpoint = get "/not-found" notFoundHandler

let redirectNotFound () =
  Response.redirectTemporarily "/not-found"

let homeContent =
  [
    // Elem.h2 [] [txt "Welcome Home"]
    Web2.breadCrumbs "Pacientes"
    Web2.Title.page
      "Gerenciador de Pacientes Novo!! (2025)"
      "Bem vindo ao gerenciador. Confira abaixo um tutorial com as novas funcionalidades."
    // Elem.div [_hypr "on load if #modal remove #modal" ] []
    ]


let emptyContent =
  [
    // Elem.h2 [] [txt "Welcome Home"]
    Web2.breadCrumbs ""
    Web2.Title.page
      "Gerenciador de Pacientes Novo!! (2025)"
      "Bem vindo ao gerenciador. Confira abaixo um tutorial com as novas funcionalidades."
    Web2.Section.main [] ]

let modalSelect (orgs: Organization list) =
  let message =
    if orgs.Length = 0 then
      Elem.p
        []
        [ _text "Você não possui nenhuma organização configurada. Por favor entre em contato com um administrador." ]
    else
      Elem.p [] [ _text "Por favor selecione uma organização" ]

  let body =
    [ message
      Elem.form
        [ _post "/select"; _target "body"; _push ]
        [ Elem.select
            [ _name "organizationId" ]
            ([ Elem.option [ Attr.disabled; Attr.value "Select" ] [ _text "Selecione" ] ]
             @ (orgs
                |> List.map (fun o -> Elem.option [ Cypher.enc o.id |> _value ] [ _text o.name ])))
          |> Web.Form.Select.wrapper
          Elem.button
            [ _class Web.Classes.saveButton; _type "submit"; _style "margin-top:8px" ]
            [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Selecionar" ] ] ] ]

  Web2.Modal.basic false "sm" "Organização" body
  |> Some

let selectPage (stx: ServiceContext) () =
  let db = stx.getDbContext ()

  let get =
    if stx.userSession.role = Role.AdminRole then
      OrganizationData.getAll db ()
    else
      OrganizationData.getByUserId db stx.userSession.userId

  async {
    let! output = get
    let s = fun o -> PageOutput.okModal "Home" emptyContent <| modalSelect o
    let f = fun o -> PageOutput.okModal "Home" emptyContent <| modalSelect []
    return either s f output
  }

module ConfirmSelect =
  let workflow (stx: ServiceContext) (id: int) =
    let db = stx.getDbContext ()

    async {
      let! org = OrganizationData.get db id

      let suc o =
        SessionManager.Instance.AddSession(stx.userSession.userId, id, o.name, stx.userSession.role)
        |> ignore

        PageOutput.Redirect "/"

      let f (err: ErrorResult) = PageOutput.errorMessage err.Message
      return either suc f org
    }

  let paramsHandler: HttpHandler =
    Request.mapForm (fun f -> f.GetString("organizationId") |> Cypher.dec)
    <| runHtmlAsync workflow

  let endpoint = post "/select" paramsHandler

let homePage stx () = PageOutput.ok "Home" homeContent

let selectList = get "/selecionar" (runHtmlAsync selectPage ())
let home = get "/" (runHtml homePage ())

let endpoints = [ home; selectList; ConfirmSelect.endpoint ]
