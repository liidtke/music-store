module Store.Domain.EmailService

open System
open Store.Domain.Email
open Store.Domain.Health


let generateResetLink baseUrl code = $"{baseUrl}/reset-password?code={code}"

let urlEncodeUri (input: string) : string = Uri.EscapeDataString(input)

let sendPasswordResetRequest
  (emailSettings: EmailSettings)
  (mailSender: MailSender)
  (htmlGenerator: RenderHtmlInput -> string)
  (uv: UserVerification)
  =
  let link = uv.hash.Value |> urlEncodeUri |> generateResetLink emailSettings.BaseUrl

  let mail =
    { link = link
      companyName = "Store"
      userName = uv.user.Value.name
      email = uv.user.Value.email }

  let html = htmlGenerator mail

  let message =
    { from = emailSettings.FromEmail
      toMail = [ uv.user.Value.email ]
      subject = "Store.- Recuperar Senha"
      body = html
      isHtml = true }

  mailSender emailSettings message


let sendPasswordChanged
  (emailSettings: EmailSettings)
  (mailSender: MailSender)
  (htmlGenerator: RenderHtmlInput -> string)
  (user:User)
  =

  let mail =
    { link = ""
      companyName = "Store."
      userName = user.name
      email = user.email }

  let html = htmlGenerator mail

  let message =
    { from = emailSettings.FromEmail
      toMail = [ user.email ]
      subject = "Store - Recuperar Senha"
      body = html
      isHtml = true }

  mailSender emailSettings message
