module Store.Domain.Email

type EmailSettings() =
    member val SmtpServer: string = "" with get, set
    member val SmtpPort: int = 0 with get, set
    // member val Username: string = "" with get, set
    member val Password: string = "" with get, set
    member val FromEmail: string = "" with get, set
    member val EnableSsl: bool = false with get, set
    member val BaseUrl: string = "" with get, set

type EmailMessage = {
    from: string
    toMail: string list
    subject: string
    body: string
    isHtml: bool
}

type RenderHtmlInput = {
  userName:string
  link:string
  email:string
  companyName:string
}

type MailSender =  EmailSettings -> EmailMessage -> Async<ServiceOutput<EmailMessage>>


