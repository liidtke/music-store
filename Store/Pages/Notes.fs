module Hs.Pages.Notes

open System
open System.Globalization
open Application
open Falco
open Hs.Components
open Hs.Domain
open Hs.Infrastructure
open Falco.Markup
open Falco.Routing
open Hs.Security

type private ModalMode =
  | CreateModal of int
  | EditModal of PatientNote
  | ReadModal of PatientNote

let private modal (mode: ModalMode) =

  let id, title, value =
    match mode with
    | CreateModal i -> i, "Adicionar Anotação", ""
    | EditModal patientNote -> patientNote.id, "Editar Anotação", patientNote.text
    | ReadModal patientNote -> patientNote.id, "Anotação", patientNote.text

  Web.Modal.basic
    true
    "md"
    title
    [ Elem.form
        [ if mode.IsCreateModal then
            _post "/part/notes"
          else
            _put "/part/notes/" ]
        [ Web.Form.Hidden.input "patient" <| Cypher.enc id
          Elem.div
            [ _style "padding: 1rem 0rem" ]
            [ Elem.div
                [ _id "note-new"
                  _name "text"
                  _hypr "on load call addQuill('#note-new', 'note__text')" ]
                [Text.raw value] ]
          Elem.input [ _name "text"; _id "note__text"; _style "display:none;";  ]
          Web.Form.actions
            [ Elem.button
                [ _class "pf-v6-c-button pf-m-link"
                  _type "button"
                  _hypr "on click remove #modal"
                  Attr.name "cancel" ]
                [ _text "Cancelar" ]
              Elem.button [ _class Web.Classes.saveButton; _type "submit"; Attr.name "submit" ] [ _text "Salvar" ] ] ] ]
    []
  |> PageOutput.modal title


module Edit =
  let rec editWorkflow (stx: ServiceContext) (id: int) =
    let db = PatientData.Notes.getNoteById <| stx.getDbContext ()

    async {
      let! result = db id

      match result with
      | Success o ->
        if o.IsNone then
          return PageOutput.errorMessage "Nota não encontrada"
        else
          return o.Value |> EditModal |> modal
      | Failure errorResult -> return PageOutput.errorMessage "Erro ao processar"
    }

  let editHandler: HttpHandler =
    Request.mapRoute (fun r -> r.GetString("id") |> Cypher.dec) <| runHtmlAsync editWorkflow

  let editEndpoint = get "/part/notes/{id}" editHandler

module Create =
  let saveWorkflow (stx: ServiceContext) (pn: PatientNote) =
    let db = stx.getDbContext ()

    let effect =
      (if pn.id = 0 then
         PatientData.Notes.insertNote
       else
         PatientData.Notes.updateNote)
        db

    async {
      let! output = PatientService.Notes.save effect stx.userSession pn

      let result =
        match output with
        | Success patientNote -> PageOutput.success "Salvo com sucesso"
        | Failure errorResult -> PageOutput.errorMessage errorResult.Message

      return result
    }

  let empty (stx: ServiceContext) (patientId: int) = patientId |> CreateModal |> modal

  let routeHandler: HttpHandler =
    Request.mapRoute (fun r -> r.GetString("id") |> Cypher.dec) (runHtml empty)

  let emptyEndpoint = get "/part/patients/{id}/empty-note" routeHandler

  let save (stx: ServiceContext) (pn: PatientNote) =
    let db = stx.getDbContext ()
    let effect = PatientData.Notes.insertNote db

    async {
      let! result = PatientService.Notes.save effect stx.userSession pn

      match result with
      | Success patientNote -> return PageOutput.successHypr "Salvo com sucesso" "on load remove #modal"
      | Failure errorResult -> return PageOutput.errorMessage errorResult.Message
    }

  let postHandler: HttpHandler =
    Request.mapForm (fun f ->
      { id = 0
        patientId = f.GetString("patient") |> Cypher.dec
        text = f.GetString("text")
        occurred = DateTime.UtcNow
        createdBy = 0

      })
    <| runHtmlAsync save

  let endpoint = post "/part/notes" postHandler

// module Details =
//   let note (stx: ServiceContext) (id: int) =
//     let effect = PatientData.Notes.getNoteById <| stx.getDbContext ()
//
//     //in the future limit by org
//     async {
//       let! output = effect <| id
//
//       return
//         match output with
//         | Success patientNote ->
//           if patientNote.IsNone then
//             PageOutput.errorMessage "Não Encontrado"
//           else
//             patientNote.Value |> ReadModal |> modal
//         | Failure errorResult -> PageOutput.errorMessage "Não Encontrado"
//     }
//
//   let routeHandler: HttpHandler =
//     Request.mapRoute (fun r -> r.GetString("id") |> Cypher.dec) (runHtmlAsync note)
//
//   let endpoint = get "/part/notes/{id}" routeHandler


module List =

  let list (pn: PatientNote list) =
    let item (i: PatientNote) =
      Elem.li
        [ Attr.class' "pf-v6-c-simple-list__item" ]
        [ Elem.button
            [ Attr.class' "pf-v6-c-simple-list__item-link"
              Attr.type' "button"
              _get $"/part/notes/{i.id |> Cypher.enc}"
              _target ".pf-v6-c-page__main-container"
              _swap "afterend" ]

            [ Text.raw <| i.occurred.ToString("dd/MM/yyyy", CultureInfo("pt-BR")) ] ]

    pn |> List.map item

  let getPatientNotes (stx: ServiceContext) (id: int) =
    let db = stx.getDbContext ()
    let effect = PatientData.Notes.getNotes db

    async {
      let! result = effect id

      match result with
      | Success patientNotes -> return patientNotes |> list |> PageOutput.ok "Notas"
      | Failure errorResult -> return PageOutput.errorMessage "Não foi possível buscar notas"
    }

  let routeHandler: HttpHandler =
    Request.mapQuery (fun r -> r.GetString("patientId") |> Cypher.dec) (runHtmlAsync getPatientNotes)

  let endpoint = get "/part/notes/" routeHandler


let endpoints =
  [ List.endpoint; Create.endpoint; Create.emptyEndpoint; Edit.editEndpoint ]
