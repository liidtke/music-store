module Hs.Pages.Configurations

open Falco.Routing
open Falco.Markup
open Application
open Hs.Components

// configure an organization


let content =
  [ Web.breadCrumbs
    Web.Title.page "Configurações" "Gerenciar sua Organização"
    Web.Tabs.wrapper [ Web.Tabs.item "Categorias" true []; Web.Tabs.item "Teste" false [] ]
    Web.Section.main
      [ Web.Tabs.body
          [ Web.Title.sec "Categorias"
            Elem.div [ _class "pf-v6-l-flex__item" ] [ Elem.p [] [ _text "teste 001 002" ] ] ] ] ]

let homeHandler stx () =
  content |> PageOutput.ok "Configurações"

let homeEndpoint = get "/config" (runHtml homeHandler ())

let endpoints = [ homeEndpoint ]
