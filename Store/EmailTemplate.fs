module Store.EmailTemplate

open Falco.Markup
open Falco.Markup.Elem
open Store.Domain.Email

let baseStyle =
  """
                    body {
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }
                    .container {
                        background-color: white;
                        padding: 30px;
                        border-radius: 8px;
                        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    }
                    .header {
                        text-align: center;
                        border-bottom: 2px solid #007bff;
                        padding-bottom: 20px;
                        margin-bottom: 30px;
                    }
                    .logo {
                        font-size: 24px;
                        font-weight: bold;
                        color: #007bff;
                    }
                    .content {
                        margin-bottom: 30px;
                    }
                    .button {
                        display: inline-block;
                        padding: 12px 30px;
                        background-color: #007bff;
                        color: white;
                        text-decoration: none;
                        border-radius: 5px;
                        font-weight: bold;
                        margin: 20px 0;
                        text-align: center;
                    }
                    .button:hover {
                        background-color: #0056b3;
                    }
                    .warning {
                        background-color: #fff3cd;
                        border: 1px solid #ffeaa7;
                        color: #856404;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 20px 0;
                    }
                    .footer {
                        text-align: center;
                        font-size: 12px;
                        color: #666;
                        border-top: 1px solid #eee;
                        padding-top: 20px;
                        margin-top: 30px;
                    }
                    .security-info {
                        background-color: #e7f3ff;
                        border-left: 4px solid #007bff;
                        padding: 15px;
                        margin: 20px 0;
                    }
                """

let attr = Attr.create
let text = Text.raw


let private passwordChangedEmailHtml (input: RenderHtmlInput) =
  html
    []
    [ head
        []
        [ meta [ Attr.create "charset" "UTF-8" ]
          meta
            [ Attr.create "name" "viewport"
              Attr.create "content" "width=device-width, initial-scale=1.0" ]
          title [] [ Text.raw "Senha Alterada" ]
          style [] [ Text.raw baseStyle ] ]
      body
        []
        [ div
            [ attr "class" "container" ]
            [ div
                [ attr "class" "header" ]
                [ div [ attr "class" "logo" ] [ text input.companyName ]
                  h1 [] [ text "Senha alterada" ] ]

              div
                [ attr "class" "content" ]
                [ p [] [ text $"Olá {input.userName}," ]

                  p [] [ text $"Sua senha foi alterada com sucesso!" ]

                  div
                    [ attr "class" "security-info" ]
                    [ h3 [] [ text "🔒 Informações de Segurança" ]
                      p [] [ text "Se você não reconhece esse acesso por favor entre em contato com nosso suporte." ] ]


                  p
                    [ attr
                        "style"
                        "word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px; font-family: monospace;" ]
                    [ text input.link ] ]

              div
                [ attr "class" "footer" ]
                [ p [] [ text $"© 2025 Store.Systems. Todos os direitos reservados." ]
                  p [] [ text "Este é um email automático, por favor não responda." ]
                  p
                    []
                    [ text "Se você tem dúvidas, entre em contato com nosso "
                      a [ attr "href" "mailto:suporte@empresa.com" ] [ text "suporte" ]
                      text "." ] ] ] ] ]

let recoverPasswordEmailHtml (input: RenderHtmlInput) =
  html
    []
    [ head
        []
        [ meta [ Attr.create "charset" "UTF-8" ]
          meta
            [ Attr.create "name" "viewport"
              Attr.create "content" "width=device-width, initial-scale=1.0" ]
          title [] [ Text.raw "Recuperação de Senha" ]
          style [] [ Text.raw baseStyle ] ]
      body
        []
        [ div
            [ attr "class" "container" ]
            [ div
                [ attr "class" "header" ]
                [ div [ attr "class" "logo" ] [ text input.companyName ]
                  h1 [] [ text "Recuperação de Senha" ] ]

              div
                [ attr "class" "content" ]
                [ p [] [ text $"Olá {input.userName}," ]

                  p
                    []
                    [ text $"Recebemos uma solicitação para redefinir a senha da sua conta: {input.email} "
                      text "Se você fez essa solicitação, clique no botão abaixo para criar uma nova senha:" ]

                  div
                    [ attr "style" "Text-align: center;" ]
                    [ a [ Attr.href input.link; attr "class" "button" ] [ text "Redefinir Senha" ] ]

                  div
                    [ attr "class" "security-info" ]
                    [ h3 [] [ text "🔒 Informações de Segurança" ]
                      p
                        []
                        [ text "Este link é válido por 2 horas e só pode ser usado uma vez. "
                          text "Para sua segurança, não compartilhe este link com ninguém." ] ]

                  div
                    [ attr "class" "warning" ]
                    [ strong [] [ text "⚠️ Importante:" ]
                      br []
                      text "Se você não solicitou a redefinição de senha, ignore este email. "
                      text "Sua senha atual permanecerá inalterada e sua conta estará segura." ]

                  p [] [ text "Caso o botão acima não funcione, copie e cole o link abaixo em seu navegador:" ]

                  p
                    [ attr
                        "style"
                        "word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px; font-family: monospace;" ]
                    [ text input.link ] ]

              div
                [ attr "class" "footer" ]
                [ p [] [ text $"© 2025 Store.Systems. Todos os direitos reservados." ]
                  p [] [ text "Este é um email automático, por favor não responda." ]
                  p
                    []
                    [ text "Se você tem dúvidas, entre em contato com nosso "
                      a [ attr "href" "mailto:suporte@empresa.com" ] [ text "suporte" ]
                      text "." ] ] ] ] ]

// Função para renderizar o HTML como string
let renderRecoverPasswordEmail input =
  recoverPasswordEmailHtml input |> renderHtml

let renderPasswordChangedEmail input =
  passwordChangedEmailHtml input |> renderHtml


let private orderPlacedEmailHtml (input: RenderHtmlInput) =
  html
    []
    [ head
        []
        [ meta [ Attr.create "charset" "UTF-8" ]
          meta
            [ Attr.create "name" "viewport"
              Attr.create "content" "width=device-width, initial-scale=1.0" ]
          title [] [ Text.raw "Pedido criado com sucesso" ]
          style [] [ Text.raw baseStyle ] ]
      body
        []
        [ div
            [ attr "class" "container" ]
            [ div
                [ attr "class" "header" ]
                [ div [ attr "class" "logo" ] [ text input.companyName ]
                  h1 [] [ text "Pedido criado" ] ]

              div
                [ attr "class" "content" ]
                [ p [] [ text $"Olá {input.userName}," ]

                  p [] [ text $"Seu pedido foi criado com sucesso!" ]

                  div
                    [ attr "class" "security-info" ]
                    [ h3 [] [ text "🔒 Informações de Segurança" ]
                      p [] [ text "Se você não reconhece esse acesso por favor entre em contato com nosso suporte." ] ]

                  p
                    [ attr
                        "style"
                        "word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px; font-family: monospace;" ]
                    [ text input.link ] ]

              div
                [ attr "class" "footer" ]
                [ p [] [ text $"© 2025 Store.Systems. Todos os direitos reservados." ]
                  p [] [ text "Este é um email automático, por favor não responda." ]
                  p
                    []
                    [ text "Se você tem dúvidas, entre em contato com nosso "
                      a [ attr "href" "mailto:suporte@empresa.com" ] [ text "suporte" ]
                      text "." ] ] ] ] ]

let orderPlacedEmail input =
  orderPlacedEmailHtml input |> renderHtml

module Professional =

  let private orderPlacedEmailHtml (input: RenderHtmlInput) =
    html
      []
      [ head
          []
          [ meta [ Attr.create "charset" "UTF-8" ]
            meta
              [ Attr.create "name" "viewport"
                Attr.create "content" "width=device-width, initial-scale=1.0" ]
            title [] [ Text.raw "Novo Pedido" ]
            style [] [ Text.raw baseStyle ] ]
        body
          []
          [ div
              [ attr "class" "container" ]
              [ div
                  [ attr "class" "header" ]
                  [ div [ attr "class" "logo" ] [ text input.companyName ]
                    h1 [] [ text "Novo Pedido Recebido" ] ]

                div
                  [ attr "class" "content" ]
                  [ p [] [ text $"Olá {input.userName}," ]

                    p [] [ text $"Há um novo pedido para você!" ]

                    div
                      [ attr "class" "security-info" ]
                      [ h3 [] [ text "🔒 Informações de Segurança" ]
                        p [] [ text "Se você não reconhece esse acesso por favor entre em contato com nosso suporte." ] ]

                    p
                      [ attr
                          "style"
                          "word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px; font-family: monospace;" ]
                      [ text input.link ] ]

                div
                  [ attr "class" "footer" ]
                  [ p [] [ text $"© 2025 Store.Systems. Todos os direitos reservados." ]
                    p [] [ text "Este é um email automático, por favor não responda." ]
                    p
                      []
                      [ text "Se você tem dúvidas, entre em contato com nosso "
                        a [ attr "href" "mailto:suporte@empresa.com" ] [ text "suporte" ]
                        text "." ] ] ] ] ]

  let orderPlacedEmail input =
    orderPlacedEmailHtml input |> renderHtml
