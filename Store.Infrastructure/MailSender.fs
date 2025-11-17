module Store.Infrastructure.MailSender

open System.Net.Mail
open System.Net
open Store.Domain.Email

let sendEmailAsync (config: EmailSettings) (message: EmailMessage) =
    async {
        try
            use smtpClient = new SmtpClient(config.SmtpServer, config.SmtpPort)
            smtpClient.Credentials <- NetworkCredential(config.FromEmail, config.Password)
            smtpClient.EnableSsl <- config.EnableSsl

            use mailMessage = new MailMessage()
            mailMessage.From <- MailAddress(message.from)
            
            message.toMail |> List.iter (fun recipient -> 
                mailMessage.To.Add(MailAddress(recipient)))
            
            mailMessage.Subject <- message.subject
            mailMessage.Body <- message.body
            mailMessage.IsBodyHtml <- true

            do! smtpClient.SendMailAsync(mailMessage) |> Async.AwaitTask
            return succeed message
        with
        | ex -> return fromException ex
    }

