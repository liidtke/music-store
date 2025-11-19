module Store.Endpoints.Handlers

let all =
  [ Login.endpoints
    Users.endpoints
    ProductTypes.endpoints
    Professionals.endpoints
    ]
  |> List.concat
