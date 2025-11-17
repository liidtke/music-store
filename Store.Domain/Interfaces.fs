module Store.Domain.Health.Interfaces

open System
open System.Data
open Microsoft.Extensions.Logging

type IRepository<'T> =
  abstract member Find: string -> Async<ServiceOutput<'T>>
  abstract member Add: 'T -> Async<ServiceOutput<'T>>
  abstract member Update: 'T -> Async<ServiceOutput<'T>>

