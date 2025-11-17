module Hs.Pages.Patients

open System
open System.Globalization
open Falco
open Falco.Routing
open Falco.Markup
open Falco.Markup.Elem
open Application
open Hs.Components
open Hs.Domain
open Hs.Infrastructure
open Hs.Security


module Listing =

  let patientLine (patients: Patient list) =
    patients
    |> List.map (fun p ->
      tr
        [ _class "pf-v6-c-table__tr" ]
        [ td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text p.fullName ]
          td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text <| if p.email.IsSome then p.email.Value else "" ]
          td [ _class "pf-v6-c-table__td pf-m-truncate" ] [ _text <| "" ]
          td
            [ _class "pf-v6-c-table__td pf-m-truncate" ]
            [ button
                [ _class "pf-v6-c-button pf-m-link"
                  _target "#page-main"
                  _push
                  _get $"/pacientes/{p.id |> Cypher.enc}" ]
                [ _text "Detalhes" ]

              ] ])

  let patientTable users =
    [ table
        [ _class "pf-v6-c-table" ]
        [ thead
            [ _class "pf-v6-c-table__thead" ]
            [ tr
                [ _class "pf-v6-c-table__tr" ]
                [ th [ _class "pf-v6-c-table__th " ] [ _text "Nome" ]
                  th [ _class "pf-v6-c-table__th pf-m-width-20" ] [ _text "Email" ]
                  th [ _class "pf-v6-c-table__th pf-m-width-20" ] [ _text "" ]
                  th [ _class "pf-v6-c-table__th pf-m-width-20" ] [] ] ]
          tbody [ _class "pf-v6-c-table__tbody" ] (patientLine users) ] ]

module Creating =

  let pageWorkflow stx () =
    [ Web.breadCrumbs
      Web.Title.page "Cadastrar Paciente" "Cadastro"
      Web.Section.main
        [ Elem.form
            [ _post "/pacientes"
              _target "#page-main"
              _push
              _class "pf-v6-c-form pf-m-limit-width" ]
            [

              Elem.div
                [ _class "pf-v6-l-grid pf-m-all-6-col-on-md pf-m-gutter" ]
                [ Web.Form.Input.empty "firstName" "firstName" "Nome" |> Web.Form.input
                  Web.Form.Input.empty "lastName" "lastName" "Sobrenome" |> Web.Form.input
                  Web.Form.Input.empty "document" "document" "Documento" |> Web.Form.input

                  //date input example
                  Web.Form.wrapper "Data de Nascimento"
                  <| Elem.span
                    [ _class "pf-v6-c-form-control" ]
                    [

                      Elem.input
                        [ _name "birthDate"
                          _id "birthDate"
                          _type "date"
                          Attr.onkeydown "dateInputKeyUp(event)"
                          Attr.placeholder "01/01/2010" ] ]

                  // Web.Form.Input.empty "birthDate" "birthDate" "Data de Nascimento" |> Web.Form.input
                  Web.Form.Input.phone "phone" "phone" "Telefone" |> Web.Form.input
                  Web.Form.Input.email "email" "email" "Email" |> Web.Form.input ]
              Web.Form.actions
                [ Elem.button
                    [ _class "pf-v6-c-button pf-m-primary"; _type "submit"; Attr.name "submit" ]
                    [ _text "Salvar" ]

                  Elem.button
                    [ _get "/pacientes"
                      _target "#page-main"
                      _push
                      _class "pf-v6-c-button pf-m-link" ]
                    [ _text "Cancelar" ] ]

              //end form
              ] ]

      ]

    |> PageOutput.ok "Cadastrar Paciente"

  type PatientInputModel =
    { id: int
      email: string option
      document: string option
      phone: string option
      birthDate: DateTime
      firstName: string
      lastName: string }

  let saveWorkflow (stx: ServiceContext) (input: PatientInputModel) =
    let db = stx.getDbContext ()
    let effect = PatientData.save db
    let service = PatientService.save effect stx.userSession

    async {
      let patient =
        { id = input.id
          org = stx.userSession.organizationId
          email = input.email
          document = input.document
          phone = input.phone
          birthDate = input.birthDate
          firstName = input.firstName
          lastName = input.lastName }
        : Patient

      let! output = patient |> service

      return
        output
        |> either //-----------------------------------------------------
          (fun p -> PageOutput.successRedirect "Salvo com Sucesso" "/pacientes") //success -------------------------------
          (fun err -> PageOutput.message "Atenção" err.Message) //failure ----------------------------------

    }


  let saveHandler: HttpHandler =
    Request.mapForm (fun f ->
      { id = 0
        email = f.TryGetString("email")
        document = f.TryGetString("document")
        phone = f.TryGetString("phone")
        birthDate = f.GetDateTime("birthDate", DateTime.MinValue)
        firstName = f.GetString("firstName", "")
        lastName = f.GetString("lastName", "") })
    <| runHtmlAsync saveWorkflow

  let pageEndpoint = get "/pacientes/cadastrar" (runHtml pageWorkflow ())
  let saveEndpoint = post "/pacientes" saveHandler

module Details =

  let addButton (p: Patient) =
    let b =
      Elem.button //how to add note
        [ Attr.class' "pf-v6-c-button pf-m-secondary"
          _get $"/part/patients/{p.id |> Cypher.enc}/empty-note"
          _target ".pf-v6-c-page__main-container"
          _swap "afterend" ]
        [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Adicionar" ] ]

    div
      [ _class "pf-v6-c-action-list" ]
      [ div [ _class "pf-v6-c-action-list__group" ] [ div [ _class "pf-v6-c-action-list__item" ] [ b ] ] ]

  let content (customer: Patient option) =

    match customer with
    | None -> []
    | Some patient ->
      [ Web.breadCrumbs
        Web.Title.page "Detalhes do Paciente" "Cadastro e Acompanhamento"
        Web.Section.main
          [ Web.toolbar
              [ button
                  [ _class "pf-v6-c-button pf-m-secondary"
                    _get "/pacientes"
                    _push
                    _target "#page-main" ]
                  [ _text "Voltar" ]
                button
                  [ _class "pf-v6-c-button pf-m-tertiary"
                    _get $"/pacientes/{patient.id |> Cypher.enc}/?mode=edit"
                    _push
                    _target "#page-main" ]
                  [ _text "Editar" ] ]


            Elem.div
              [ _class "pf-v6-l-grid pf-m-all-6-col-on-md pf-m-gutter" ]
              [ Elem.div //column left
                  []
                  [

                    Web.Description.wrapper
                      [ Web.Description.group "Nome" patient.firstName
                        Web.Description.group "Sobrenome" patient.lastName
                        Web.Description.group "Data de Nascimento"
                        <| patient.birthDate.ToString("dd/MM/yyyy", CultureInfo("pt-BR"))

                        Web.Description.group "Email" <| stringToDisplay patient.email
                        Web.Description.group "Documento" <| stringToDisplay patient.document ] ] //end column

                Elem.div
                  []
                  [ Web.Title.h3 "Anotações"
                    addButton patient
                    Elem.div
                      [ Attr.class' "pf-v6-c-simple-list"; _style "margin-top: 4px" ]
                      [ Elem.ul
                          [ Attr.class' "pf-v6-c-simple-list__list"
                            Attr.role "list"
                            _get $"/part/notes/?patientId={patient.id |> Cypher.enc}"
                            _trigger "load"
                            _inner ]
                          []
                        //end div
                        ]
                    //end c-simple-list
                    ] ] ]
        // Web.divider
        // Web.Section.secondary [ Web.Title.sec "Valores a Receber"; Elem.div [] [ _text "Teste 001" ] ]

        Elem.div
          [ _id "tabs" ]
          [ Web.Tabs.wrapper
              [ Web.Tabs.item "Agendamentos" false [ _target "#tabs"; _get "/pacientes/agendamentos" ]
                Web.Tabs.item "Valores a Receber" false [ _target "#tabs"; _get "/pacientes/valores" ] ]
            Web.Section.mainWith [ _id "subtab" ] [] ]

        ]

  let details (stx: ServiceContext) (id: int) =
    let effect = PatientData.getPatient <| stx.getDbContext ()

    async {
      let! output = effect id

      return
        output
        |> either //-------------------------------
          (fun s -> s |> content |> PageOutput.ok "Detalhes do Paciente")
          (fun err ->
            [ Web.breadCrumbs
              Web.Title.page "Detalhes do Paciente" "Cadastro e Acompanhamento"
              Web.Section.main [ Elem.div [] [ _text "Não Encontrado" ] ] ]
            |> PageOutput.ok "Detalhes do Paciente")


    }

  let detailsHandler: HttpHandler =
    Request.mapRoute (fun i -> i.GetString("id") |> Cypher.dec)
    <| runHtmlAsync details

  let endpoint = get "/pacientes/{id}" detailsHandler

module Tabs =

  //por ids nas requests
  let values (stx: ServiceContext) id =
    Elem.div
      [ _id "tabs" ]
      [ Web.Tabs.wrapper
          [ Web.Tabs.item "Agendamentos" false [ _target "#tabs"; _get "/pacientes/agendamentos" ]
            Web.Tabs.item "Valores a Receber" true [ _target "#tabs"; _get "/pacientes/valores" ] ]
        Web.Section.mainWith [ _id "subtab" ] [ Web.Title.h3 "Valores a Receber ou Recebidos" ] ]
    |> PageOutput.single

  let valuesEndpoint = get "/pacientes/valores" (runHtml values ())


  let scheduling (stx: ServiceContext) id =
    Elem.div
      [ _id "tabs" ]
      [ Web.Tabs.wrapper
          [ Web.Tabs.item "Agendamentos" true [ _target "#tabs"; _get "/pacientes/agendamentos" ]
            Web.Tabs.item "Valores a Receber" false [ _target "#tabs"; _get "/pacientes/valores" ] ]
        Web.Section.mainWith [ _id "subtab" ] [ Web.Title.h3 "Agendamentos Pelo usuário - Historico e etc" ] ]
    |> PageOutput.single

  let endpoint = get "/pacientes/agendamentos" (runHtml scheduling ())

let content =
  [ Web.breadCrumbs
    Web.Title.page "Pacientes" "Cadastro e Acompanhamento"
    Web.Section.main
      [ Web.toolbar
          [ Elem.a
              [ _class "pf-v6-c-button pf-m-primary"; _boost; _href "/pacientes/cadastrar" ]
              [ _text "Cadastrar Paciente" ]
            Web.Search.searchFilter
              [ Attr.name "search"
                _get "/part/patients"
                Attr.placeholder "Buscar"
                _searchTrigger
                _target "#s01" ] ]
        // Web.divider
        div [ _id "s01"; _class ""; _get "/part/patients"; _trigger "load" ] [] ] ]

let patientsResult (stx: ServiceContext) (text: string) =
  let db = stx.getDbContext ()
  let effect = PatientData.searchPatient db

  async {
    let! result = effect text stx.userSession.organizationId

    return
      result
      |> either //-------------------------------------------------
        (fun patients -> patients |> Listing.patientTable |> PageOutput.ok "Pacientes")
        (fun err -> PageOutput.message "Atenção" "Não foi possível buscar os pacientes")

  }

let searchUsersHandler ctx =
  let q = Request.getQuery ctx
  let search = q.GetString("search", "")
  runHtmlAsync patientsResult search ctx

let patientsEndpoint = get "/part/patients" searchUsersHandler


let home stx () = PageOutput.ok "Pacientes" content

let homeEndpoint = get "/pacientes" (runHtml home ())

let endpoints =
  [ homeEndpoint
    patientsEndpoint
    Details.endpoint
    Tabs.endpoint
    Tabs.valuesEndpoint
    Creating.pageEndpoint
    Creating.saveEndpoint ]
