module Store.Endpoints.Handlers

let all =
  [ Login.endpoints
    Users.endpoints
    ]
  |> List.concat
