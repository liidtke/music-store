module Hs.Pages.Organizations

open Falco
open Falco.Routing
open Falco.Markup
open Falco.Markup.Elem
open Application
open Hs.Components
open Hs.Domain
open Hs.Infrastructure
open Hs.Security
open Microsoft.Extensions.Logging

let content =
  [ Web.breadCrumbs
    Web.Title.page "Organizações" "Gerenciar"
    Web.Section.main
      [ Web.toolbar
          [ button
              [ _class "pf-v6-c-button pf-m-primary"
                _get "/orgs/cadastrar"
                _target "#page-main"
                _push
                _trigger "click" ]
              [ _text "Cadastrar Organização" ] ]
        div [ _class ""; _id "orgs"; _get "/part/orgs"; _trigger "load" ] [] ] ]

let orgList (orgs: Organization list) =
  let lines o =
    tr
      [ _class "pf-v6-c-table__tr" ]
      [ td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text o.name ]
        // td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text o.slug ]
        td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text <| o.createdBy.ToString() ] //convert to model list, include name
        td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text <| if o.isActive then "Ativo" else "Inativo" ]
        td
          [ _class "pf-v6-c-table__td pf-m-truncate" ]
          [ button
              [ _class "pf-v6-c-button pf-m-link"
                _target "#page-main"
                _push
                _get $"/orgs/{o.id |> Cypher.enc}" ]
              [ _text "Detalhes" ] ] ]

  let head =
    thead
      [ _class "pf-v6-c-table__thead" ]
      [ tr
          [ _class "pf-v6-c-table__tr" ]
          [ th [ _class "pf-v6-c-table__th pf-m-width-20" ] [ _text "Nome" ]
//            th [ _class "pf-v6-c-table__th pf-m-width-20" ] [ _text "Raiz (uri)" ]
            th [ _class "pf-v6-c-table__th pf-m-width-20" ] [ _text "Criador" ]
            th [ _class "pf-v6-c-table__th pf-m-width-20" ] [ _text "Ativo" ]
            th [ _class "pf-v6-c-table__th pf-m-width-20" ] []

            ] ]

  table [ _class "pf-v6-c-table pf-m-grid-md pf-m-compact" ] [ head; tbody [ _class "pf-v6-c-table__tbody" ] (orgs |> List.map lines) ]
  |> _toList

module Details =
  open Hs.Domain.OrganizationService
  let a = 0

  //todo add https://www.patternfly.org/components/inline-edit
  let page (org: Organization) =
    [ Web.breadCrumbs
      Web.Title.page "Detalhes da Organização" org.name
      Web.Section.main
        [ Web.Description.wrapper
            [ div
                [ _id "org-name"; _class "pf-v6-l-grid pf-m-all-6-col-on-md pf-m-gutter" ]
                [ Web.Description.group "Nome" org.name
                  Web.InlineEdit.def [ _get $"/part/orgs/{org.id |> Cypher.enc}/edit-name"; _target "#org-name" ] ]
              Web.Description.group "Ativação" <| if org.isActive then "Ativa" else "Inativa"
              // Web.Description.group "Link" org.slug
              ]

          Elem.br [] ]
      Web.Section.page
        "org-content"
        [ Web.Title.h2 "Usuários"
          Elem.div
            [ _class "pf-v6-l-grid pf-m-all-6-col-on-md pf-m-gutter" ]
            [ Elem.div
                [ _class ".pf-v6-l-grid__item" ]
                [ Elem.h3 [ _class "pf-v6-c-content--h3" ] [ _text "Adicionar Usuários" ]
                  form
                    []
                    [ org.id |> Cypher.enc |> Web.Form.Hidden.input "id"
                      Web.Search.search
                        [ _name "search"
                          _include "closest form"
                          _get "/part/orgs/users/"
                          _target "#user-search"
                          _searchTrigger
                          _placeholder "Pesquisar Usuários"
                          _autocomplete "off" ] ]
                  Elem.div
                    [ _id "user-search" ]
                    [

                    ] ]
              Elem.div
                [ _class ".pf-v6-l-grid__item" ]
                [ Elem.h3 [ _class "pf-v6-c-content--h3" ] [ _text "Usuários Vinculados" ]
                  Elem.div
                    [ _id "user-list"
                      _get $"/part/orgs/{org.id |> Cypher.enc}/users"
                      _trigger "load" ]
                    [

                    ] ] ] ] ]


  let workflow (stx: ServiceContext) (cid: string) =
    let id = Cypher.dec cid
    let effect = OrganizationData.get <| stx.getDbContext ()

    async {
      let! output = effect id

      match output with
      | Success organization -> return organization |> page |> PageOutput.ok "Detalhes"
      | Failure errorResult -> return PageOutput.PageNotFound
    }

  //user-search
  let userList (oid: string) (toAdd: bool) (a: User list) =
    let lines (u: User) =
      let uid = Cypher.enc u.id
      let formId = "form-" + (if toAdd then "a" else "r") + uid

      tr
        [ _class "pf-v6-c-table__tr" ]
        [ td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text u.name ]
          td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text u.email ]
          td
            [ _class "pf-v6-c-table__td pf-v6-c-table__action" ]
            [ button [ _class "pf-v6-c-button pf-m-link"; _type "submit"; Attr.form formId ] [ if toAdd then _text "Adicionar" else _text "Remover" ]
              form
                [ _id formId; _target "#user-list"; _patch $"/orgs/{oid}" ]
                [ Web.Form.Hidden.input "userId" uid
                  Web.Form.Hidden.input "operation" <| if toAdd then "add" else "remove" ] ] ]

    let head =
      thead
        [ _class "pf-v6-c-table__thead" ]
        [ tr
            [ _class "pf-v6-c-table__tr" ]
            [ th [ _class "pf-v6-c-table__th " ] [ _text "Nome" ]
              th [ _class "pf-v6-c-table__th " ] [ _text "Email" ]
              th [ _class "pf-v6-c-table__th pf-v6-c-table__cell-empty" ] []

              ] ]

    if a.Length > 0 then
      table [ _class "pf-v6-c-table pf-m-grid-md pf-m-compact" ] [ head; tbody [ _class "pf-v6-c-table__tbody" ] (a |> List.map lines) ]
      |> _toList
    else
      [ div [ _class "" ] [ _text "Nenhum resultado" ] ]
  // let items = a |> List.map (fun i -> Elem.li [] [ _text i.name ])
  // [ Elem.ul [] items ]

  let workflowUserList (stx: ServiceContext) id =
    let effect = stx.getDbContext () |> UserData.getByOrgId

    async {
      let! result = effect <| Cypher.dec id

      match result with
      | Success s -> return s |> userList id false |> PageOutput.ok "Detalhes"
      | Failure f -> return [] |> userList id false |> PageOutput.ok "Detalhes"
    }

  type OrgUserSearchInput = { id: string; search: string }

  let workflowUserSearch (stx: ServiceContext) (input: OrgUserSearchInput) =
    let effects = stx.getDbContext () |> UserData.searchUser

    async {
      let! result = effects input.search

      match result with
      | Success s -> return s |> userList input.id true |> PageOutput.ok "Detalhes"
      | Failure f -> return [] |> userList input.id true |> PageOutput.ok "Detalhes"
    }

  type OrganizationPatchInput =
    { id: string
      userId: string
      toAdd: bool }

  let patchWorkflow (stx: ServiceContext) (input: OrganizationPatchInput) =
    async {
      let db = stx.getDbContext ()
      let oid = input.id |> Cypher.dec
      let userId = input.userId |> Cypher.dec
      let updateEffect = OrganizationData.updateUsers db
      let getEffect = OrganizationData.get db
      let getUsersEffect = UserData.getByOrgId db

      let service = OrganizationService.updateUser updateEffect getEffect stx.userSession

      let! output =
        { organizationId = oid
          userId = userId
          op =
            if input.toAdd then
              UserOperation.AddUser
            else
              UserOperation.RemoveUser }
        |> service

      match output with
      | Success organization ->
        let! usersOutput = getUsersEffect oid

        match usersOutput with
        | Success users -> return users |> userList input.id false |> PageOutput.ok "Detalhes"
        | Failure errorResult -> return PageOutput.errorMessage errorResult.Message
      | Failure errorResult -> return PageOutput.errorMessage errorResult.Message
    }

  type AlterOrganizationInput = { id: string; name: string }

  //put
  let putWorkflow (stx: ServiceContext) (input: AlterOrganizationInput) =
    async {
      let db = stx.getDbContext ()
      let oid = input.id |> Cypher.dec
      let saveEffect = OrganizationData.update db
      let getEffect = OrganizationData.get db
      let getUsersEffect = UserData.getByOrgId db

      db.logger.LogInformation($"updating org with {input.name}")

      let service = OrganizationService.update saveEffect getEffect stx.userSession

      let! result =
        { id = oid
          name = input.name
          isActive = true
          users = []
          createdBy = 0 }
        |> service

      match result with
      | Success organization -> return PageOutput.successRedirect "Organização alterada com sucesso" $"/orgs/{input.id}"
      | Failure errorResult -> return PageOutput.errorMessage "Erro ao salvar"

    }

  let putFormHandler id : HttpHandler =
    Request.mapForm (fun q ->
      let name = q.GetString("name", "")

      { id = id; name = name }: AlterOrganizationInput)
    <| runHtmlAsync putWorkflow

  let putRouteHandler: HttpHandler =
    Request.mapRoute (fun i ->
      let a = i.GetString("id", "")
      a)
    <| putFormHandler

  //end put

  //edit name
  let workflowEditName (stx: ServiceContext) id =
    let ret (org: Organization) =
      [ form [ _put $"/orgs/{id}" ] [ Web.InlineEdit.inlineEdit "name" "Nome" org.name [ _type "submit" ] [_get $"/orgs/{id}"; _target "#page-main"; _push] ] ]
      |> PageOutput.ok "Editar"

    async {
      let oid = Cypher.dec id
      let db = stx.getDbContext () |> OrganizationData.get
      let! o = db oid

      match o with
      | Success organization -> return ret organization
      | Failure errorResult -> return PageOutput.message "Atenção" "Erro ao buscar"

    }

  let paramsHandlerEditName: HttpHandler =
    Request.mapRoute (fun i -> i.GetString("id", ""))
    <| runHtmlAsync workflowEditName

  //end edit name

  let routeHandler: HttpHandler =
    Request.mapRoute (fun i ->
      let a = i.GetString("id", "")
      a)
    <| runHtmlAsync workflow

  let paramsHandlerUserList: HttpHandler =
    Request.mapRoute (fun i -> i.GetString("id", ""))
    <| runHtmlAsync workflowUserList

  let paramsHandlerUserSearch: HttpHandler =
    Request.mapQuery (fun i ->
      { search = i.GetString("search", "")
        id = i.GetString("id", "") })
    <| runHtmlAsync workflowUserSearch

  let patchFormHandler id : HttpHandler =
    Request.mapForm (fun q ->
      let userId = q.GetString("userId", "")
      let op = q.GetString("operation", "add")

      { id = id
        userId = userId
        toAdd = op = "add" })
    <| runHtmlAsync patchWorkflow

  let patchHandler: HttpHandler =
    Request.mapRoute (fun i ->
      let a = i.GetString("id", "")
      a)
    <| patchFormHandler

  let endpointPut = put "/orgs/{id}/" putRouteHandler
  let endpointPatch = patch "/orgs/{id}/" patchHandler
  let endpointUserList = get "/part/orgs/{id}/users" paramsHandlerUserList
  let endpointEditName = get "/part/orgs/{id}/edit-name" paramsHandlerEditName
  let endpointUserSearch = get "/part/orgs/users" paramsHandlerUserSearch
  let endpoint = get "/orgs/{id}" routeHandler


module Create =
  open Hs.Domain.OrganizationService

  let backButton =
    button
      [ _class "pf-v6-c-button pf-m-secondary"
        _get "/orgs"
        _trigger "click"
        _target "#page-main"
        _push ]
      [ _text "Voltar" ]

  let createForm =
    Elem.form
      [ Attr.id "create-form"
        _class "pf-v6-c-form pf-m-limit-width"
        _post "/orgs"
        _trigger "submit"
        _target "#page-main" ]
      [ Web.Form.Input.empty "name" "name" "Nome" |> Web.Form.input
        // Web.Form.Input.empty "slug" "slug" "Raiz (uri)" |> Web.Form.input

        Web.Form.actions
          [ a
              [ _class "pf-v6-c-button pf-m-link"
                _type "button"
                Attr.name "cancel"
                _href "/orgs/" ]
              [ _text "Cancelar" ]
            button [ _class Web.Classes.saveButton; _type "submit"; Attr.name "submit" ] [ _text "Salvar" ] ] ]

  let createPage =
    [ Web.breadCrumbs
      Web.Title.page "Organizações" "Cadastrar"
      Web.Section.main [ Web.toolbar [ backButton ] ]
      Web.Section.main [ createForm ] ]

  let postSave id =
    createPage
    @ [ div
          [ _get $"/orgs/"
            _trigger "load delay:2s"
            _target "#page-main"
            _push ]
          []
        Web.Toaster.success "Salvo com sucesso" "Organização salva, redirecionando..."
        |> Web.Toaster.wrapper ]

  let workflow (stx: ServiceContext) input =
    let saveEffect = stx.getDbContext () |> OrganizationData.save

    async {
      let! result = create saveEffect stx.userSession input

      match result with
      | Success s -> return postSave s.id |> PageOutput.ok "Org"
      | Failure f -> return f.Message |> PageOutput.message "Validação"
    }

  let formHandler: HttpHandler =
    Request.mapForm (fun f ->
      { name = f.GetString("name", "")
        slug = f.GetString("slug", "") })
    <| runHtmlAsync workflow

  let createOrgEndpoint = post "/orgs" formHandler

let partList (stx: ServiceContext) () =
  let effect = stx.getDbContext () |> OrganizationData.getAll

  async {
    let! orgs = effect ()

    match orgs with
    | Success organizations -> return organizations |> orgList |> PageOutput.ok "Lista"
    | Failure errorResult -> return PageOutput.GenericError errorResult.Message
  }

let createHandler stx () =
  Create.createPage |> PageOutput.ok "Cadastrar Organização"

let createEndpoint = get "/orgs/cadastrar" (runHtml createHandler ())
let listEndpoint = get "/part/orgs" (runHtmlAsync partList ())

let home stx () = PageOutput.ok "Organizações" content

let homeEndpoint = get "/orgs" (runHtml home ())

let endpoints =
  [ homeEndpoint
    listEndpoint
    createEndpoint
    Create.createOrgEndpoint
    Details.endpoint
    Details.endpointPatch
    Details.endpointUserSearch
    Details.endpointPut
    Details.endpointEditName
    Details.endpointUserList ]
