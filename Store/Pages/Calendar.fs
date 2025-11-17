module Hs.Pages.Calendar

open System.Globalization
open Falco.Markup
open System
open Hs.Components


type CalDay = { day: int; fade: bool }
type CalWeek = CalDay list
type CalMonth = CalWeek list

let generateWeek (initial: DateTime) index =
  let factor = (index * 7) + (initial.DayOfWeek |> int) * -1
  let date = factor |> initial.AddDays
  let max = DateTime.DaysInMonth(date.Year, date.Month)

  seq {
    for i in [ 0..6 ] ->
      let next = date.AddDays(i)
      let fade = initial.Month <> next.Month
      { day = next.Day; fade = fade }
  }
  |> Seq.toList

let generateMonth (initial: DateTime) =
  seq { for i in [ 0..4 ] -> generateWeek initial i } |> Seq.toList


let days w =
  let clName d =
    "day " + (if d.fade then "faded" else "visible")

  w
  |> List.map (fun d -> Elem.div [ _class <| clName d ] [ _text <| d.day.ToString() ])

let calRows (month: CalMonth) =
  month |> List.map (fun w -> Elem.div [ _class "week" ] (days w))

let calendar (requestedDate: DateTime) =
  let date = DateTime(requestedDate.Year, requestedDate.Month, 1)

  let w =
    [ Elem.div
        [ _class "week header" ]
        [ Elem.div [ _class "day header" ] [ _text "Domingo" ]
          Elem.div [ _class "day header" ] [ _text "Segunda" ]
          Elem.div [ _class "day header" ] [ _text "Terça" ]
          Elem.div [ _class "day header" ] [ _text "Quarta" ]
          Elem.div [ _class "day header" ] [ _text "Quinta" ]
          Elem.div [ _class "day header" ] [ _text "Sexta" ]
          Elem.div [ _class "day header" ] [ _text "Sábado" ] ] ]

  let days = date |> generateMonth |> calRows
  let monthName = date.ToString("MMMM", CultureInfo.InvariantCulture)

  Elem.div
    [ _class "calendar" ]
    [ Elem.div [ _class "calendar-header" ] [ _text $"{date.Year} {monthName}" ]
      Elem.div [ _class "calendar-body" ] (w @ days) ]
