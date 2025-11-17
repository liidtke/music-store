module Store.Infrastructure.Access.Models

type AppSystem =
  | Health
  | Store

module AppSystem =
  let toInt = function
    | AppSystem.Health -> 1
    | AppSystem.Store  -> 2

  let ofInt = function
    | 1 -> AppSystem.Health
    | 2 -> AppSystem.Store
    | x -> failwithf "Valor inválido para AppSystem: %i" x

type ResourceData =
  { id: int
    code: string
    name: string
    system: int }

