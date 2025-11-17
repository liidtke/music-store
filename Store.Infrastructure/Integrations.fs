module Store.Infrastructure.Integrations

open System.Text.Json

type UserProfile =
  { sub: string
    givenName: string //aldo
    familyName: string //liidtke
    nickname: string
    name: string //Aldo Liidtke
    picture: string
    updatedAt: string
    email: string
    emailVerified: bool }

module internal FlurlBuilder =
  open Flurl
  open Flurl.Http

  let jsonOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)

  let getJson<'T> (request: IFlurlRequest) =
    request.GetJsonAsync<'T>() |> Async.AwaitTask

module Auth0 =
  open Flurl
  open Flurl.Http

  let baseUrl = "https://dev-1stgqvkfim2oq8qq.us.auth0.com" //todo settings this
  let userInfo = baseUrl + "/userinfo"

  let getProfile token =
    async {
      try
        let! result = userInfo.WithOAuthBearerToken(token) |> FlurlBuilder.getJson<UserProfile>
        return succeed result
      with ex ->
        return internalError ex
    }
