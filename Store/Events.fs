module Store.Events

open Store.Domain
open Store.Domain.Email
open Store.Domain.EmailService
open Microsoft.Extensions.Logging
open Store.Domain.Interfaces
open Store.Domain.Store.Catalog
open Store.Domain.Store.Ordering
open Store.Infrastructure
open Wolverine

let send (bus: IMessageBus) (output: DomainOutput<'T, 'TEvent>) =
  let send e =
    async {
      do! bus.PublishAsync(e).AsTask() |> Async.AwaitTask
      return output
    }

  if output.SendEvent then
    send output.event
  else
    async { return output }


let sendToCreator (emailService: EmailService) (event: OrderPlacedEvent) =
  let baseEmail =
    { userName = event.order.customer.name
      link = "${emailService.BaseUrl}/orders/"
      email = event.order.customer.email
      companyName = "Music Store" }

  let mailSender = MailSender.sendEmailAsync
  let body = EmailTemplate.orderPlacedEmail baseEmail

  let message =
    { from = ""
      toMail = [ event.order.customer.email ]
      subject = "Music Store - Pedido Criado"
      body = body
      isHtml = true }

  async {

    let! _ = emailService.Send(mailSender, message)
    return ()
  }


let sendToProfessional (emailService: EmailService) (order: Order) (p: User) =
  let baseEmail =
    { userName = p.name
      link = $"{emailService.BaseUrl}/orders/{order.id}"
      email = p.email
      companyName = "Music Store" }

  let mailSender = MailSender.sendEmailAsync
  let body = EmailTemplate.Professional.orderPlacedEmail baseEmail

  let message =
    { from = ""
      toMail = [ order.customer.email ]
      subject = "Music Store - Você tem um novo pedido"
      body = body
      isHtml = true }

  async {
    let! _ = emailService.Send(mailSender, message)
    return succeed p
  }

let sendToAllProfessionals (emailService: EmailService) (order: Order) (ur: IUserRepository) =
  let ids =
    order.items |> List.map (fun p -> p.product.professionalId) |> List.distinct

  async {
    let! users = ids |> List.map (fun id -> ur.Find(id)) |> Async.Sequential

    let sendByUser (u: User ServiceOutput) =
      match u with
      | Success u -> sendToProfessional emailService order u |> Async.RunSynchronously |> ignore
      | Failure _ -> ()

    users |> Seq.toList |> List.iter (fun u -> sendByUser u)
    return ()
  }

type OrderPlacedEventHandler
  (logger: ILogger<OrderPlacedEventHandler>, emailService: EmailService, ur: IUserRepository) =
  member _.Handle(event: OrderPlacedEvent) =

    task {
      let! _ = sendToCreator emailService event
      do! sendToAllProfessionals emailService event.order ur

      return ()
    }
