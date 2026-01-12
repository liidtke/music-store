module Store.Endpoints.Handlers

let all =
  [ Login.endpoints
    Users.endpoints
    ProductTypes.endpoints
    Professionals.endpoints
    Products.endpoints
    Samples.endpoints
    Baskets.endpoints
    Customers.endpoints
    ]
  |> List.concat
