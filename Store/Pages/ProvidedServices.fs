module Hs.Pages.ProvidedServices

open System
open System.Globalization
open Application
open Hs.Domain
open Hs.Domain.AccountsReceivable
open Falco
open Falco.Routing
open Hs.Components
open Falco.Markup
open Hs.Domain.AccountsReceivableService
open Hs.Infrastructure
open Hs.Security

let innerIcon c =
  Elem.span [ _class "pf-v6-c-form-control__utilities" ] [ Elem.span [ _class "pf-v6-c-form-control__icon" ] [ Elem.i [ _class c ] [] ] ]

let toggleGroup (date: DateTime) =
  let previous = date.AddMonths(-1)
  let next = date.AddMonths(1)

  Elem.div
    [ Attr.class' "pf-v6-c-toggle-group" ]
    [
      // First item
      Elem.div
        [ Attr.class' "pf-v6-c-toggle-group__item" ]
        [ Elem.button
            [ Attr.class' "pf-v6-c-toggle-group__button"
              Attr.type' "button"
              _get $"/valores?month={previous.Month}&year={previous.Year}"
              _target "#page-main"
              _push ]
            [ Elem.span [ Attr.class' "pf-v6-c-toggle-group__icon" ] [ Elem.i [ Attr.class' "fas fa-arrow-left" ] [] ]
              Elem.span [ Attr.class' "pf-v6-c-toggle-group__text" ] [ Text.raw "Anterior" ] ] ]
      // Second item
      Elem.div
        [ Attr.class' "pf-v6-c-toggle-group__item" ]
        [ Elem.button
            [ Attr.class' "pf-v6-c-toggle-group__button"; Attr.type' "button" ]
            [ Elem.span [ Attr.class' "pf-v6-c-toggle-group__icon" ] [ Elem.i [ Attr.class' "fas fa-calendar" ] [] ]
              Elem.span [ Attr.class' "pf-v6-c-toggle-group__text" ] [ Text.raw <| date.ToString("MMMM", CultureInfo("pt-BR")) ] ] ]
      // Third item (Share)
      Elem.div
        [ Attr.class' "pf-v6-c-toggle-group__item" ]
        [ Elem.button
            [ Attr.class' "pf-v6-c-toggle-group__button"
              Attr.type' "button"
              _get $"/valores?month={next.Month}&year={next.Year}"
              _target "#page-main"
              _push ]
            [ Elem.span [ Attr.class' "pf-v6-c-toggle-group__icon" ] [ Elem.i [ Attr.class' "fas fa-arrow-right" ] [] ]
              Elem.span [ Attr.class' "pf-v6-c-toggle-group__text" ] [ Text.raw "Próximo" ] ] ]

      // end buttons
      Elem.form
        [ _id "date-form" ]
        [ Web.Form.Hidden.input "month" <| date.Month.ToString()
          Web.Form.Hidden.input "year" <| date.Year.ToString() ] ]


let addDay month year patient =
  Elem.form
    [ _class "pf-v6-l-grid__item pf-m-3-col"
      _id "empty-day"
      _post ("/values/")
      _target "#empty-day"
      _swap "beforebegin" ]
    [

      Web.Form.Hidden.input "year" (year.ToString())
      Web.Form.Hidden.input "patient" patient
      Web.Form.Hidden.input "month" (month.ToString())
      Elem.div
        [ _class "pf-v6-l-grid pf-m-all-6-col-on-md pf-m-gutter" ]
        [ Elem.div
            [ _class "pf-v6-c-form__group" ]
            [ Elem.div
                [ _class "pf-v6-c-form__group-label" ]
                [
                  // Elem.i [ _class "fas fa-calendar"; _style "margin-right:8px" ] []
                  Elem.label [ _class "pf-v6-c-form__label" ] [ _text "Dia" ] ]
              Elem.div
                [ _class "pf-v6-c-form__group-control" ]
                [ Elem.span
                    [ _class "pf-v6-c-form-control pf-m-required pf-m-icon" ]
                    [
                      //day input
                      Elem.input [ _name "day"; _type "number"; _hypr "on keyup call numberChanged(me)" ]
                      innerIcon "fas fa-calendar"

                      ] ] ]
          Elem.div
            [ _class "pf-v6-c-form__group" ]
            [ Elem.div
                [ _class "pf-v6-c-form__group-label" ]
                [
                  // Elem.i [ _class "fas fa-dollar-sign"; _style "margin-right:8px" ] []
                  Elem.label [ _class "pf-v6-c-form__label" ] [ _text "Valor" ] ]
              Elem.div
                [ _class "pf-v6-c-form__group-control" ]
                [
                  Elem.span
                    [ _class "pf-v6-c-form-control pf-m-required pf-m-icon" ]
                    [
                      //value input
                      Elem.input [ _name "value"; Attr.onkeydown "valueInputKeyUp(event)"; Attr.value "R$ 0,00" ]
                      innerIcon "fas fa-dollar-sign"

                      ] ] ]
          Elem.select
            [ _name "status" ]
            [ Elem.option
                [ ProvidedServiceStatus.Pending |> ProvidedServiceStatus.toString |> _value
                  Attr.selected ]
                [ ProvidedServiceStatus.Pending |> ProvidedServiceStatus.toString |> _text ]
              Elem.option [ ProvidedServiceStatus.Paid |> ProvidedServiceStatus.toString |> _value ] [ ProvidedServiceStatus.Paid |> ProvidedServiceStatus.toString |> _text ] ]
          |> Web.Form.Select.wrapper
          |> Web.Form.wrapper "Status"
          //end select
          Elem.div
            [ _class "pf-v6-c-form__group" ]
            [ Elem.div [ _class "pf-v6-c-form__group-label" ] [ Elem.label [ _class "pf-v6-c-form__label" ] [ _text "Valor Líquido" ] ]
              Elem.div
                [ _class "pf-v6-c-form__group-control" ]
                [ Elem.span
                    [ _class "pf-v6-c-form-control pf-m-required pf-m-icon" ]
                    [
                      //value input
                      Elem.input
                        [ _name "netValue"
                          Attr.onkeydown "valueInputKeyUp(event)"
                          Attr.value "R$ 0,00" ]
                      innerIcon "fas fa-dollar-sign"

                      ] ] ]

          Elem.div
            [ _class "pf-v6-l-grid__item pf-m-6-col"
              _hypr "on click remove closest #empty-day" ]
            [ Elem.button
                [ Attr.class' "pf-v6-c-button pf-m-link pf-m-danger pf-m-block"
                  Attr.type' "button" ]
                [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Cancelar" ] ] ]
          Elem.div
            [ _class "pf-v6-l-grid__item pf-m-6-col" ]
            [ Elem.button
                [ Attr.class' "pf-v6-c-button pf-m-link pf-m-unread pf-m-block"
                  Attr.type' "submit" ]
                [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Salvar" ] ] ]
          //end grid
          ] ]


let editDay (ps: ProvidedService) =
  let internalId = $"item-{ps.id |> Cypher.enc}"

  Elem.form
    [ _class "pf-v6-l-grid__item pf-m-3-col"; _id internalId; _put ("/values/") ]
    [

      Web.Form.Hidden.input "id" <| ps.id.ToString()
      Web.Form.Hidden.input "patient" <| ps.patient.id.ToString()
      Web.Form.Hidden.input "month" <| ps.serviceDate.Month.ToString()
      Web.Form.Hidden.input "year" <| ps.serviceDate.Year.ToString()

      Elem.div
        [ _class "pf-v6-l-grid pf-m-all-6-col-on-md pf-m-gutter" ]
        [ Elem.div
            [ _class "pf-v6-c-form__group" ]
            [ Elem.div [ _class "pf-v6-c-form__group-label" ] [ Elem.label [ _class "pf-v6-c-form__label" ] [ _text "Dia" ] ]
              Elem.div
                [ _class "pf-v6-c-form__group-control" ]
                [ Elem.span
                    [ _class "pf-v6-c-form-control pf-m-required pf-m-icon" ]
                    [
                      //day input
                      Elem.input
                        [ _name "day"
                          _type "number"
                          _hypr "on keyup call numberChanged(me)"
                          _value <| ps.serviceDate.Day.ToString() ]
                      innerIcon "fas fa-calendar"

                      ] ] ]
          Elem.div
            [ _class "pf-v6-c-form__group" ]
            [ Elem.div
                [ _class "pf-v6-c-form__group-label" ]
                [
                  // Elem.i [ _class "fas fa-dollar-sign"; _style "margin-right:8px" ] []
                  Elem.label [ _class "pf-v6-c-form__label" ] [ _text "Valor" ] ]
              Elem.div
                [ _class "pf-v6-c-form__group-control" ]
                [ Elem.span
                    [ _class "pf-v6-c-form-control pf-m-required pf-m-icon" ]
                    [
                      //value input
                      Elem.input
                        [ _name "value"
                          Attr.onkeydown "valueInputKeyUp(event)"
                          ps.value |> Money.toString |> Attr.value ]
                      innerIcon "fas fa-dollar-sign"

                      ] ] ]

          Elem.select
            [ _name "status" ]
            [ Elem.option
                [ ProvidedServiceStatus.Pending |> ProvidedServiceStatus.toString |> _value
                  if ps.status = Pending then Attr.selected else () ]
                [ ProvidedServiceStatus.Pending |> ProvidedServiceStatus.toString |> _text ]
              Elem.option
                [ ProvidedServiceStatus.Paid |> ProvidedServiceStatus.toString |> _value
                  if ps.status = Paid then Attr.selected else () ]
                [ ProvidedServiceStatus.Paid |> ProvidedServiceStatus.toString |> _text ]
              Elem.option
                [ ProvidedServiceStatus.Canceled |> ProvidedServiceStatus.toString |> _value
                  if ps.status = Canceled then Attr.selected else () ]
                [ ProvidedServiceStatus.Canceled |> ProvidedServiceStatus.toString |> _text ] ]
          |> Web.Form.Select.wrapper
          |> Web.Form.wrapper "Status"

          Elem.div
            [ _class "pf-v6-c-form__group" ]
            [ Elem.div [ _class "pf-v6-c-form__group-label" ] [ Elem.label [ _class "pf-v6-c-form__label" ] [ _text "Valor Líquido" ] ]
              Elem.div
                [ _class "pf-v6-c-form__group-control" ]
                [ Elem.span
                    [ _class "pf-v6-c-form-control pf-m-required pf-m-icon" ]
                    [
                      //value input
                      Elem.input
                        [ _name "netValue"
                          Attr.onkeydown "valueInputKeyUp(event)"
                          ps.netValue |> Money.toString |> Attr.value ]
                      innerIcon "fas fa-dollar-sign"

                      ] ] ]

          Elem.div
            [ _class "pf-v6-l-grid__item pf-m-6-col" ]
            [ Elem.button
                [ Attr.class' "pf-v6-c-button pf-m-link pf-m-danger pf-m-block"
                  Attr.type' "button"
                  _get $"/values/{ps.id |> Cypher.enc}"
                  _outer
                  _target $"#{internalId}" ]
                [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Cancelar" ] ] ]
          Elem.div
            [ _class "pf-v6-l-grid__item pf-m-6-col" ]
            [ Elem.button
                [ Attr.class' "pf-v6-c-button pf-m-link pf-m-unread pf-m-block"
                  Attr.type' "submit" ]
                [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Salvar" ] ] ]
          //end grid
          ] ]

let gridItem (i: ProvidedService) =
  let internalId = $"item-{i.id |> Cypher.enc}"

  Elem.div
    [ _class "pf-v6-l-grid__item"; _id internalId ]
    [ Elem.div
        [ _class "pf-v6-l-grid pf-m-all-6-col-on-sm pf-m-all-12-col-on-md" ]
        [ Elem.div
            [ _class "pf-v6-l-grid__item " ]
            [ Elem.i [ _class "fas fa-calendar"; _style "margin-right:6px" ] []
              Elem.label [] [ _text "Dia" ] ]
          Elem.div [ _class "pf-v6-l-grid__item pf-m-12-col" ] [ Elem.p [ _style "margin-left: 18px;" ] [ _text <| i.serviceDate.Day.ToString() ] ]
          Elem.div
            [ _class "pf-v6-l-grid__item " ]
            [ Elem.i [ _class "fas fa-dollar-sign"; _style "margin-right:6px" ] []
              Elem.label [] [ _text "Valor" ] ]

          Elem.div [ _class "pf-v6-l-grid__item pf-m-12-col" ] [ Elem.p [ _style "margin-left: 16px;" ] [ i.value |> Money.toString |> _text ] ]

          Elem.div
            [ _class "pf-v6-l-grid__item " ]
            [ if i.status = ProvidedServiceStatus.Paid then
                Elem.i [ _class "fas fa-check"; _style "margin-right:6px" ] []
              else
                Elem.i [ _class "fas fa-exclamation-triangle"; _style "margin-right:6px" ] []
              Elem.label [] [ i.status |> ProvidedServiceStatus.toString |> _text ] ]

          Elem.div
            [ _class "pf-v6-l-grid__item"; _style "margin-top: 4px;" ]
            [ Elem.button
                [ _class "pf-v6-c-button pf-m-block pf-m-link pf-m-small"
                  _get $"/values/{Cypher.enc i.id}/edit"
                  _outer
                  _target $"#{internalId}" ]
                [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Editar" ] ] ]

          //end grid
          ] ] //todo format

let days (p: PatientService) (date: DateTime) (lines: ProvidedService list) =
  let id = p.id |> Cypher.enc

  Elem.div
    [ _class "pf-v6-l-grid pf-m-gutter pf-m-all-6-col-on-sm pf-m-all-2-col-on-lg pf-m-all-1-col-on-xl pf-m-all-4-col-on-md"
      _id $"line-{id}" ]
  <|
  //for every day
  (lines |> List.map gridItem)
  @ [ Elem.div
        [ _class "pf-v6-l-grid__item"; _id $"line-{id}-add" ]
        [ Elem.button
            [ _class "pf-v6-c-button pf-m-control pf-m-small"
              _get $"/part/values/empty/?p={id}&m={date.Month}&y={date.Year}"
              _target $"#line-{id}-add"
              _select "#empty-day"
              _swap "beforebegin" ]
            [ Elem.span [ _class "pf-v6-c-button__icon" ] [ Elem.i [ _class "fas fa-plus" ] [] ] ] ] ]


let dataItem date providedServices (patient: PatientService) =
  let items =
    providedServices
    |> List.where (fun i -> i.patient.id = patient.id)
    |> List.sortBy (fun i -> i.serviceDate.Date.Day)

  //list of items inside data list
  Elem.li
    [ Attr.class' "pf-v6-c-data-list__item" ]
    [ Elem.div
        [ Attr.class' "pf-v6-c-data-list__item-row" ]
        [ Elem.div
            [ Attr.class' "pf-v6-c-data-list__item-content" ]
            [ Elem.div
                [ Attr.class' "pf-v6-c-data-list__cell pf-m-align-left pf-m-flex-1" ]
                [ Elem.div
                    [ Attr.class' "pf-v6-l-flex pf-m-column pf-m-space-items-md" ]
                    [ Elem.div
                        [ Attr.class' "pf-v6-l-flex pf-m-column pf-m-space-items-none" ]
                        [ Elem.div [ Attr.class' "pf-v6-l-flex__item" ] [ Elem.p [ Attr.id "data-list-basic-example-data-list-item-1" ] [ Text.raw patient.name ] ]
                          Elem.div [ Attr.class' "pf-v6-l-flex__item" ] [ Elem.small [] [ Text.raw "" ] ] ]

                      ]

                  ]
              Elem.div
                [ _class "pf-v6-c-data-list__cell pf-m-align-left pf-m-flex-3" ]
                [ Elem.div [ _class "pf-v6-l-flex pf-m-column pf-m-space-items-md" ] [ days patient date items ] ]
              Elem.div
                [ Attr.class' "pf-v6-c-data-list__cell pf-m-align-right pf-m-no-fill" ]
                [
                  // [ Elem.button [ Attr.class' "pf-v6-c-button pf-m-link"; Attr.type' "button" ] [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Adicionar" ] ]
                  Elem.button
                    [ Attr.class' "pf-v6-c-button pf-m-link pf-m-danger"; Attr.type' "button" ]
                    [ Elem.span [ Attr.class' "pf-v6-c-button__text" ] [ Text.raw "Remover" ] ]

                  //end div
                  ] ] ] ]

//https://www.patternfly.org/components/data-list/html
let dataList (date: DateTime) (providedServices: ProvidedService list) =
  let patients = providedServices |> List.map (fun i -> i.patient) |> List.distinct

  patients
  |> List.map (dataItem date providedServices)
  |> Elem.ul [ Attr.class' "pf-v6-c-data-list"; Attr.role "list"; Attr.id "data-list" ]

//end html components

module EmptyPart =
  type EmptyInput =
    { month: int
      year: int
      patientName: string
      patient: string //id
      line: string }

  let emptyWorkflow (stx: ServiceContext) (input: EmptyInput) =
    if input.line = "day" then
      addDay input.month input.year input.patient |> PageOutput.single
    else
      dataItem
      <| DateTime(input.year, input.month, 01)
      <| []
      <| { id = input.patient |> Cypher.dec
           name = input.patientName }
      |> PageOutput.single

  let queryHandler =
    Request.mapQuery (fun q ->
      { month = q.GetInt("m", 1)
        year = q.GetInt("y", 2025)
        line = q.GetString("line", "day")
        patientName = q.GetString("name", "")
        patient = q.GetString("p", "") })
    <| runHtml emptyWorkflow

  let emptyEndpoint = get "/part/values/empty/" queryHandler

module Creating =
  type CreateProvidedService =
    { patient: string
      day: int
      month: int
      value: decimal
      netValue: decimal
      status: string
      year: int }

  let workflow (stx: ServiceContext) (i: CreateProvidedService) =
    let db = stx.getDbContext ()
    let saveEffect = ProvidedServiceData.insert db
    let fetchEffect = ProvidedServiceData.exists db

    let service =
      AccountsReceivableService.create stx.userSession saveEffect fetchEffect

    let input =
      { id = 0
        value = i.value
        netValue = i.netValue
        patientId = i.patient |> Cypher.dec
        serviceDate = DateTime(i.year, i.month, i.day)
        status = i.status |> ProvidedServiceStatus.convert }
      : AccountsReceivableService.CreateProvidedServiceInput

    async {
      let! result = service input

      return
        match result with
        | Success providedService -> providedService |> gridItem |> elemToList |> PageOutput.ok "Teste"
        | Failure errorResult -> PageOutput.message "Não foi possível salvar" errorResult.Message
    }

  let formHandler =
    Request.mapForm (fun f ->
      { patient = f.GetString("patient")
        day = f.GetInt("day", 1)
        year = f.GetInt("year", 2025)
        month = f.GetInt("month", 1)
        value = f.GetString("value", "0,00") |> Money.parse
        netValue = f.GetString("netValue", "0,00") |> Money.parse
        status = f.GetString("status", "Pendente") })
    <| runHtmlAsync workflow
  // let queryHandler = Request.mapRoute (fun q -> q.GetString("id", "")) formHandler

  let endpoint = post "/values/" formHandler

module Editing =

  type UpdateProvidedService =
    { id: int
      patient: string
      day: int
      month: int
      value: decimal
      netValue: decimal
      status: string
      year: int }

  let workflow (stx: ServiceContext) (input: UpdateProvidedService) =
    let db = stx.getDbContext ()
    let saveEffect = ProvidedServiceData.update db
    let fetchEffect = ProvidedServiceData.get db

    let service =
      AccountsReceivableService.update stx.userSession saveEffect fetchEffect

    let serviceInput: CreateProvidedServiceInput =
      { id = input.id
        patientId = Cypher.dec input.patient
        serviceDate = DateTime(input.year, input.month, input.day)
        status = input.status |> ProvidedServiceStatus.convert
        value = input.value
        netValue = input.netValue }

    async {
      let! result = service serviceInput

      match result with
      | Success providedService -> return providedService |> gridItem |> PageOutput.single
      | Failure errorResult -> return PageOutput.errorMessage errorResult.Message
    }

  let formHandler =
    Request.mapForm (fun f ->
      { id = f.GetInt("id", 0)
        patient = f.GetString("patient")
        day = f.GetInt("day", 1)
        year = f.GetInt("year", 2025)
        month = f.GetInt("month", 1)
        value = f.GetString("value", "0,00") |> Money.parse
        netValue = f.GetString("netValue", "0,00") |> Money.parse
        status = f.GetString("status", "Pending") })
    <| runHtmlAsync workflow

  let endpoint = put "/values/" formHandler

  let editWorkFlow (stx: ServiceContext) (id: int) =
    let db = stx.getDbContext ()
    let fetchEffect = ProvidedServiceData.get db

    let successFunc s =
      match s with
      | Some v -> v |> editDay |> PageOutput.single
      | None -> PageOutput.errorMessage "Não encontrado"

    let errFunc (e: ErrorResult) = PageOutput.errorMessage e.Message

    async {
      let! result = fetchEffect id
      return result |> either successFunc errFunc
    }

  let editHandler =
    Request.mapRoute (fun r -> r.GetString("id", "") |> Cypher.dec)
    <| runHtmlAsync editWorkFlow

  let editEndpoint = get "/values/{id}/edit" editHandler

  let getWorkFlow (stx: ServiceContext) id =
    let db = stx.getDbContext ()
    let fetchEffect = ProvidedServiceData.get db

    let successFunc s =
      match s with
      | Some v -> v |> gridItem |> PageOutput.single
      | None -> PageOutput.errorMessage "Não encontrado"

    let errFunc (e: ErrorResult) = PageOutput.errorMessage e.Message

    async {
      let! result = fetchEffect id
      return result |> either successFunc errFunc
    }

  let getHandler =
    Request.mapRoute (fun r -> r.GetString("id", "") |> Cypher.dec)
    <| runHtmlAsync getWorkFlow

  let getEndpoint = get "/values/{id}" getHandler

module internal SelectPatient =
  type internal SelectInput =
    { search: string
      month: int
      year: int
      name: string }

  let workflowSearch (stx: ServiceContext) (search: SelectInput) =
    let db = stx.getDbContext ()
    let qu = PatientData.searchPatient db

    let list_item (p: Patient) =
      Elem.li
        [ Attr.class' "pf-v6-c-simple-list__item" ]
        [ Elem.button
            [ Attr.class' "pf-v6-c-simple-list__item-link pf-m-current"
              Attr.type' "button"
              _get $"/part/values/empty/?p={p.id |> Cypher.enc}&m={search.month}&y={search.year}&name={p.fullName}&line=line"
              _target $"#data-list"
              _swap "afterbegin" ]
            [ Text.raw p.fullName ] ]

    let su (ps: Patient list) =
      Elem.form
        [ _style "" ]
        [ Elem.div
            [ Attr.class' "pf-v6-c-simple-list" ]
            [ (if ps.Length = 0 then
                 [ Elem.p [] [ _text "Nenhum item encontrado" ] ]
               else
                 ps |> List.map list_item)
              |> Elem.ul [ Attr.class' "pf-v6-c-simple-list__list"; Attr.role "list" ] ] ]
      |> PageOutput.single

    let err e =
      PageOutput.errorMessage "Não foi possível buscar"

    async {
      let! result = qu search.search stx.userSession.organizationId
      return result |> either su err
    }

  let searchHandler: HttpHandler =
    Request.mapQuery (fun q ->
      { search = q.GetString("search", "")
        name = q.GetString("name", "")
        month = q.GetInt("month", 01)
        year = q.GetInt("year", 2025) })
    <| runHtmlAsync workflowSearch

  let endpointSearch = get "part/values/patients" searchHandler

  let workflow stx _ =
    Web.Modal.basic
      true
      "md" 
      "Selecionar Paciente"
      [ Elem.form
          []
          [ Web.Search.search
              [ _name "search"
                _autofocus
                _placeholder "Nome ou Email"
                _autocomplete "off"
                _searchTrigger
                _get "/part/values/patients"
                _include "#date-form"
                _target "#inside" ] ]
        Elem.div [ _id "inside"; _style "margin-top:12px" ] [] ]
      []
    |> PageOutput.modal "Selecionar Paciente"

  let endpoint = get "part/values/add" (runHtml workflow ())

module Reporting =
  type internal ReportInput =
    { patientId: string option
      month: int
      year: int }

  let internal workflowSearch (stx: ServiceContext) (search: ReportInput) =
    let db = stx.getDbContext ()
    let qu = ProvidedServiceData.getReports db

    async {
      let! result =
        qu
          { orgId = stx.userSession.organizationId
            date = DateTime(search.year, search.month, 1)
            patientId =
              if search.patientId.IsSome then
                Cypher.dec search.patientId.Value |> Some
              else
                None }

      let suc (vals:ProvidedServiceMonthResult list) =
        let okValue = vals |> List.where (fun i -> i.status = Paid) |> List.sumBy (fun i -> i.value)
        let okNetValue = vals |> List.where (fun i -> i.status = Paid) |> List.sumBy (fun i -> i.netValue)
        let pendingValue = vals |> List.where (fun i -> i.status = Pending) |> List.sumBy (fun i -> i.value)
        let pendingNetValue = vals |> List.where (fun i -> i.status = Pending) |> List.sumBy (fun i -> i.netValue)
        
        PageOutput.ok
          ""
          [ Elem.div
              [ _id "result"; _class "pf-v6-l-flex" ]
              [
                //start fields 
                Elem.div
                  [ _class "pf-v6-l-flex__item" ]
                  [ Elem.strong [] [ _text "Valor Recebido:" ]
                    Elem.span [ _style "margin-left:4px" ] [ okValue |> Money.toString |> _text  ] ]
                Elem.div
                  [ _class "pf-v6-l-flex__item" ]
                  [ Elem.strong [] [ _text "Valor Líquido:" ]
                    Elem.span [ _style "margin-left:4px" ] [ okNetValue |> Money.toString |> _text  ] ]
                Elem.div
                  [ _class "pf-v6-l-flex__item" ]
                  [ Elem.strong [] [ _text "Valor Pendente:" ]
                    Elem.span [ _style "margin-left:4px" ] [  pendingValue |> Money.toString |> _text ] ] ]
            //end item
            ]

      return result |> either suc (fun e -> PageOutput.errorMessage "erro")
    }

  let searchHandler: HttpHandler =
    Request.mapQuery (fun q ->
      { patientId = q.TryGetString("search")
        month = q.GetInt("month", 01)
        year = q.GetInt("year", 2025) })
    <| runHtmlAsync workflowSearch

  let searchEndpoint = get "/values/report" searchHandler

module Listing =
  type ListParams = { month: int; year: int }

  let page date ps =
    [ Breadcrumbs.standard [ "Valores a Receber", "/valores" ]
      Web.Title.page "Valores a Receber" "Lista de valores e de serviços prestados a receber"
      Web.Section.main
        [ Web.toolbar
            [ Elem.button
                [ _class "pf-v6-c-button pf-m-primary"
                  _get "part/values/add"
                  _target ".pf-v6-c-page__main-container"
                  _swap "afterend"
                  _trigger "click" ]
                [ _text "Adicionar Paciente" ]
              toggleGroup date
              Elem.div
                [ _get "/values/report"
                  _class "toolbar_item"
                  _include "#date-form"
                  _trigger "load"
                  _inner
                  _select "#result" ]
                [] ]
          dataList date ps ] ]

  let workflow (stx: ServiceContext) (p: ListParams) =
    let date =
      if p.month = 0 || p.year = 0 then
        DateTime.Now
      else
        DateTime(p.year, p.month, 01)

    async {
      let! results =
        ProvidedServiceData.search
        <| stx.getDbContext ()
        <| { orgId = stx.userSession.organizationId
             date = date
             patientId = None }

      let items =
        match results with
        | Success ps -> ps
        | Failure errorResult -> []

      return items |> page date |> PageOutput.ok "Valores a Receber"
    }

  let paramsHandler: HttpHandler =
    Request.mapQuery (fun i ->
      { year = i.GetInt("year", 0)
        month = i.GetInt("month", 0) })
    <| runHtmlAsync workflow

  let endpoint = get "/valores" paramsHandler

let endpoints =
  [ Listing.endpoint
    EmptyPart.emptyEndpoint
    Creating.endpoint
    Editing.endpoint
    Editing.editEndpoint
    Editing.getEndpoint
    Reporting.searchEndpoint
    SelectPatient.endpointSearch
    SelectPatient.endpoint ]
