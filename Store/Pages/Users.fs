module Hs.Pages.Users

open System.Threading.Tasks
open Falco
open Falco.Routing
open Falco.Markup
open Application
open Hs.Components
open Hs.Components.Web.Form
open Hs.Domain
open Hs.Domain.UserService.Effects
open Hs.Infrastructure
open Hs.Security

module Details =
  open Falco.Markup.Elem

  let roleSelect (selected: Role) =
    let genOptions (selected: Role) (items: Role list) =
      items
      |> List.map (fun i ->
        option
          [ i.ToString() |> Attr.value
            if selected = i then Attr.selected else () ]
          [ i |> Models.Role.toString |> _text ])

    let options =
      genOptions selected [ Role.CustomerRole; Role.ManagerRole; Role.PhysicianRole; Role.AdminRole ]

    select [ Attr.name "role" ]
    <| [ option [ Attr.disabled; Attr.value "CustomerRole" ] [ _text "Selecione" ] ]
       @ options
    // option [ Role.CustomerRole.ToString() |> Attr.value; ] [ Role.CustomerRole |> Models.Role.toString |> _text ]
    // option [ Role.AdminRole.ToString() |> Attr.value ] [ Role.AdminRole |> Models.Role.toString |> _text ]
    // option [ Role.ManagerRole.ToString() |> Attr.value ] [ Role.ManagerRole |> Models.Role.toString |> _text ]
    // option [ Role.PhysicianRole.ToString() |> Attr.value ] [ Role.PhysicianRole |> Models.Role.toString |> _text ] ]
    |> Web.Form.Select.wrapper
    |> Web.Form.wrapper "Perfil"


  let editPage (user: User) =
    let f =
      form
        [ _class "pf-v6-c-form pf-m-limit-width"
          _id "put-user"
          _target "#page-main"
          _put $"/usuarios/{user.id |> Cypher.enc}" ]
        [ Web.Form.input
            { label = "Nome"
              name = "name"
              id = "n"
              inputType = "text"
              isReadOnly = false
              placeholder = None
              value = Some(user.name)
              isRequired = true }
          Web.Form.input
            { label = "Email"
              name = "email"
              id = "e"
              inputType = "email"
              isReadOnly = true
              placeholder = Some("email@site.com")
              value = Some(user.email)
              isRequired = true }
          roleSelect user.role ]

    [ Web.breadCrumbs
      Web.Title.page "Editar Detalhes do Usuário" user.name
      Web.Section.main
        [ Web.toolbar
            [ button
                [ _class Web.Classes.cancelButton
                  _push
                  _target "#page-main"
                  _get $"/usuarios/{user.id |> Cypher.enc}" ]
                [ _text "Cancelar" ]
              button [ _class Web.Classes.saveButton; _type "submit"; Attr.form "put-user" ] [ _text "Salvar" ] ]
          f ]
      Web.divider
      Web.Section.main [ Web.Title.sec "ETC" ] ]



  let page (user: User) =
    [ Web.breadCrumbs
      Web.Title.page "Detalhes do Usuário" user.name
      Web.Section.main
        [ Web.toolbar
            [ button
                [ _class "pf-v6-c-button pf-m-tertiary"
                  _get $"/usuarios/{user.id |> Cypher.enc}/?mode=edit"
                  _push
                  _target "#page-main" ]
                [ _text "Editar" ] ]


          Web.Description.wrapper
            [ Web.Description.group "Nome" user.name
              Web.Description.group "Email" user.email
              Web.Description.group "Verificado"
              <| if user.isVerified then "Verificado" else "Não Verificado"
              Web.Description.group "Status"
              <| if user.isDeleted then "Desabilitado" else "Habilitado"
              Web.Description.group "Perfil" <| Models.Role.toString user.role ] ]
      Web.divider
      Web.Section.main [ Web.Title.sec "ETC" ]

      ]

  let showPage (user: User) (mode: string) =
    if (mode = "edit") then editPage user else page user

  let notFound = [ Web.Empty.notFound "Usuário não Encontrado" "/usuarios" ]

  type UserDetailsInput = { id: string; mode: string }

  let workflow (stx: ServiceContext) (input: UserDetailsInput) =
    let id = Cypher.dec input.id
    let db = stx.getDbContext () |> UserData.factory

    async {
      let! userOutput = db.get id

      match userOutput with
      | Success user -> return showPage user input.mode |> PageOutput.ok "Detalhes do Usuário"
      | Failure f -> return PageOutput.customValidation notFound
    }

  let updateWorkflow (stx: ServiceContext) (input: User) =
    let db = stx.getDbContext () |> UserData.factory
    let saveEffect = fun user -> async { return succeed user }
    let countEffect _ = async { return succeed 0 }
    let secSet = stx.securitySettings
    let encryptEffect = hashPassword secSet

    async {
      let! output = UserService.Update.handle stx.userSession db input

      match output with
      | Success user -> return showPage user "" |> PageOutput.ok "Detalhes do Usuário"
      | Failure f -> return PageOutput.message "Erro ao salvar" f.Message
    }

  let paramsHandler input : HttpHandler =
    Request.mapQuery (fun i ->
      let a = i.GetString("mode", "")
      { input with mode = a })
    <| runHtmlAsync workflow

  let routeHandler: HttpHandler =
    Request.mapRoute (fun i ->
      let a = i.GetString("id", "")
      { mode = ""; id = a })
    <| paramsHandler

  let updateFormHandler input : HttpHandler =
    Request.mapForm (fun i ->
      let user: User =
        { id = Cypher.dec input
          email = i.GetString("email", "")
          name = i.GetString("name", "")
          role = i.GetString("role", "UserRole") |> Role.convert
          password = None
          isDeleted = i.GetBoolean("isDeleted", false)
          isVerified = i.GetBoolean("isVerified", false) }

      user)
    <| runHtmlAsync updateWorkflow

  let updateRouteHandler: HttpHandler =
    Request.mapRoute (fun i ->
      let a = i.GetString("id", "")
      a)
    <| updateFormHandler

  let updateEndpoint = put "/usuarios/{id}" updateRouteHandler
  let endpoint = get "/usuarios/{id}" routeHandler

module List =
  open Falco.Markup.Elem

  type private ListInput = { success: bool; search: string }

  let usersLine (users: User list) =
    users
    |> List.map (fun u ->
      tr
        [ _class "pf-v6-c-table__tr" ]
        [ td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text u.name ]
          td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text u.email ]
          td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text <| u.role.ToString() ]
          td
            [ _class "pf-v6-c-table__td pf-m-truncate" ]
            [ button
                [ _class "pf-v6-c-button pf-m-link"
                  _target "#page-main"
                  _push
                  _get $"/usuarios/{u.id |> Cypher.enc}" ]
                [ _text "Detalhes" ]

              ] ])

  let usersTable users =
    [ table
        [ _class "pf-v6-c-table" ]
        [ thead
            [ _class "pf-v6-c-table__thead" ]
            [ tr
                [ _class "pf-v6-c-table__tr" ]
                [ th [ _class "pf-v6-c-table__th " ] [ _text "Nome" ]
                  th [ _class "pf-v6-c-table__th pf-m-width-20" ] [ _text "Email" ]
                  th [ _class "pf-v6-c-table__th pf-m-width-20" ] [ _text "Perfil" ]
                  th [ _class "pf-v6-c-table__th pf-m-width-20" ] [] ] ]
          tbody [ _class "pf-v6-c-table__tbody" ] (usersLine users) ] ]

  let usersDetails so =
    match so with
    | Success u -> usersTable u
    | _ -> []

  let private workflow (stx: ServiceContext) (listInput: ListInput) =
    let searchUserEffect = stx.getDbContext () |> UserData.searchUser

    async {
      do! Task.Delay(500) |> Async.AwaitTask
      let! searchOutput = searchUserEffect listInput.search
      return searchOutput |> usersDetails |> PageOutput.ok "Usuários"
    }

  let handler: HttpHandler =
    runHtmlAsync workflow
    |> Request.mapQuery (fun q ->
      { success = q.GetBoolean("success", false)
        search = q.GetString("search", null) })

  let endpoint = get "/part/users" handler

module Create =
  let page (message: string option) =
    [ Web.breadCrumbs
      Web.Title.page "Usuários" "Cadastrar Novo Usuário"
      // Web.Section.main
      //     [ Web.toolbar
      //           [ Elem.button
      //                 [ _class "pf-v6-c-button pf-m-secondary"
      //                   _get "/usuarios"
      //                   _target "#page-main"
      //                   _push ]
      //                 [ _text "Voltar" ] ] ]
      Web.Section.main
        [ Elem.form
            [ _post "/usuarios"
              _target "#page-main"
              _push
              _class "pf-v6-c-form pf-m-limit-width" ]
            [ Web.Form.Input.empty "name" "name" "Nome Completo" |> Web.Form.input
              Web.Form.Input.email "email" "email" "Email" |> Web.Form.input
              Web.Form.actions
                [ Elem.button
                    [ _class "pf-v6-c-button pf-m-primary"; _type "submit"; Attr.name "submit" ]
                    [ _text "Salvar" ]
                  Elem.button
                    [ _get "/usuarios"
                      _target "#page-main"
                      _push
                      _class "pf-v6-c-button pf-m-link" ]
                    [ _text "Cancelar" ] ] ] ] ]

  let workflow stx () = page None |> PageOutput.ok "Cadastrar"

  let confirm =
    [ Elem.p [] [ _text "Cadastro concluído com sucesso." ]
      Elem.a [ _class "button"; Attr.href "/login" ] [ _text "Efetuar Login" ] ]

  let createEmptyEndpoint = get "/usuarios/cadastrar" (runHtml workflow ())

  let confirmEndpoint = //todo finish
    get "/confirmar-cadastro" (confirm |> Html.html "home" "" |> Response.ofHtml)

  let createWorkflow (stx: ServiceContext) user =
    let db = stx.getDbContext () |> UserData.factory
    let secSet = stx.securitySettings
    let encryptEffect = hashPassword secSet

    let deps: Dependencies =
      { count = db.count
        save = db.save
        get = db.get }

    async {
      let! output = UserService.Create.handle stx.userSession encryptEffect deps user

      match output with
      | Success o -> return "/usuarios" |> PageOutput.successRedirect "Sucesso!"
      | Failure errorResult -> return errorResult.Message |> PageOutput.message "Validação"
    }


  let createHandler: HttpHandler =
    Request.mapForm (fun f ->
      let email = f.GetString("email")
      let role = f.GetString("role")
      let name = f.GetString("name")

      { email = email
        name = name
        role = role }
      : UserService.Create.CreateUser)

    <| runHtmlAsync createWorkflow

  let createEndpoint = post "/usuarios" createHandler

let content success =
  [ Web.breadCrumbs
    Web.Title.page "Usuários" "Cadastros e Permissões"
    Web.Section.main
      [ Web.toolbar
          [ Elem.button
              [ _get "/usuarios/cadastrar"
                _push
                _target "body"
                _class "pf-v6-c-button pf-m-primary" ]
              [ _text "Cadastrar Usuário" ]

            Web.Search.searchFilter
              [ _name "search"
                _get "/part/users"
                _placeholder "Buscar"
                _searchTrigger
                _target "#user-list" ] ]

        Elem.div [ _id "user-list"; _class ""; _get "/part/users"; _trigger "load" ] [ Web.Skeleton.table ] ]

    if success then
      Web.Section.main [ Web.Toaster.success "Sucesso" "Usuário criado com sucesso" ]
    else
      Elem.div [] []
    // [ Elem.button
    //       [ acl "secondary"
    //         hxGet "/usuarios/novo"
    //         hxTarget "#aside-panel"
    //         hxSwap "innerHTML"
    //         hypr "on click call toggleAside()" ]
    //       [ txt "Cadastrar Novo Usuário" ] ]
    ]

// module CreateNew =
//     let content =
//         Elem.div
//             [ acl "p-panel"; aid "new-panel" ]
//             [ Elem.h3 [] [ txt "Cadastrar Novo Usuário" ]
//               // Elem.p [] [ txt "Here" ]
//               Elem.form
//                   [ hxPost "/usuarios"; hxTarget "#new-panel" ]
//                   [ Elem.label [] [ txt "Nome Completo" ]
//                     Elem.input [ Attr.type' "text"; Attr.name "name" ]
//                     Elem.label [] [ txt "Email" ]
//                     Elem.input [ Attr.type' "email"; Attr.name "email" ]
//                     Elem.label [] [ txt "Senha Temporária" ]
//                     Elem.input [ Attr.type' "password"; Attr.name "pwd"; Attr.autocomplete "false" ]
//                     Elem.button [ acl "primary"; atype "submit"; Attr.name "submit" ] [ txt "Salvar" ]
//                     Elem.button [ atype "submit"; Attr.name "submit"; hypr "on click call toggleAside()" ] [ txt "Cancelar" ] ]
//
//              ]


let home (stx: ServiceContext) s = PageOutput.ok "Usuários" <| content s

let homeHandler: HttpHandler =
  Request.mapQuery (fun q -> q.GetBoolean("success", false)) <| runHtml home

let homeEndpoint = get "/usuarios" homeHandler

let endpoints =
  [ homeEndpoint
    Create.createEmptyEndpoint
    Create.confirmEndpoint
    Create.createEndpoint
    Details.endpoint
    Details.updateEndpoint
    List.endpoint ]
