module Store.Endpoints.Handlers

let all =
  [ Login.endpoints
    Users.endpoints
    ProductTypes.endpoints
    ]
  |> List.concat
