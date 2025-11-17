module Store.Program

open Store.Configurations
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Falco
open Falco.Routing


let configureCors (corsBuilder: CorsPolicyBuilder) : unit =
  corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod() |> ignore

let configureApp: IApplicationBuilder -> IApplicationBuilder =
  fun app ->
    app.UseCors(fun options -> configureCors options) |> ignore
    app.UseStaticFiles(StaticFileOptions(RequestPath = "/static")) |> ignore
    app.UseAuthentication() |> ignore
    app.UseAuthorization() |> ignore

    app

let configureWebHost (configuration: IConfiguration) (webHost: IHostBuilder) =
  webHost.ConfigureServices(Configurations.configureServices configuration)

let endpoints =
  [ get "/_version" (Response.ofPlainText "Hello world") ]
  @ Endpoints.Handlers.all


[<EntryPoint>]
let main args =

  let bldr = WebApplication.CreateBuilder()
  configureServices bldr.Configuration bldr.Services |> ignore
  Configurations.configureEventBus bldr |> ignore

  let wapp = bldr.Build()

  // let conf =
  //   bldr.Configuration
  //     .AddJsonFile("appsettings.json", optional = false)
  //     .AddJsonFile("appsettings.Development.json")

  wapp
    .Use(configureApp)
    .UseRouting()
    .UseFalco(endpoints)
    // .UseFalcoNotFound(Pages.App.notFoundHandler)
    .Run()

  0
