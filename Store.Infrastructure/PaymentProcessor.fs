module Store.Infrastructure.PaymentProcessor

open Store.Domain.Store.Interfaces
open Store.Domain.Store.Ordering

type PaymentProcessor() =
  interface IPaymentProcessor with
    member _.Process(order: Order) = async { return order |> succeed }
