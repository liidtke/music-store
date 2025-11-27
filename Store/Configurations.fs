module Store.Configurations

open System
open System.Text
open Store.Domain
open Store.Domain.Email
open Store.Domain.Interfaces
open Store.Domain.Store.Interfaces
open Store.Endpoints.Queries
open Store.Infrastructure.Access
open Store.Infrastructure.Access.UserRepository
open Store.Infrastructure.Access.UserVerificationRepository
open Store.Infrastructure.Context
open Store.Infrastructure.Store
open Store.Infrastructure.Store.ProductRepository
open Store.Infrastructure.Store.ProductTypeRepository
open Store.Infrastructure.Store.ProfessionalRepository
open Store.Infrastructure.Store.SampleRepository
open Store.Session
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Store.Security
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.Tokens
open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

open Wolverine
open Wolverine.MemoryPack
open Wolverine.RabbitMQ


let configureEventBus (host: WebApplicationBuilder) =
  let rabbitUrl = host.Configuration.GetConnectionString("RabbitMqUrl")

  host.UseWolverine(fun w ->
    w.UseRabbitMq(Uri(rabbitUrl)).EnableWolverineControlQueues().UseConventionalRouting().AutoProvision()
    |> ignore

    w.UseMemoryPackSerialization() |> ignore

  )

let configureServices (configuration: IConfiguration) (services: IServiceCollection) =
  // let currentUser = CurrentUser.standard

  services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"))
  |> ignore

  services.AddSingleton<EmailSettings>(
    System.Func<System.IServiceProvider, EmailSettings>(fun sp ->
      sp.GetRequiredService<IOptions<EmailSettings>>().Value)
  )
  |> ignore

  let section = configuration.GetSection("Security")

  let sec: SecuritySettings =
    { Iterations = section.GetValue<int>("Iterations")
      JwtSecurityKey = section.GetValue<string>("JwtSecurityKey")
      JwtIssuer = section.GetValue<string>("JwtIssuer")
      JwtAudience = section.GetValue<string>("JwtAudience")
      TokenExpirationTime = section.GetValue<int>("TokenExpirationTime") }

  let validationParameters: TokenValidationParameters =
    new TokenValidationParameters(
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidAudience = sec.JwtAudience,
      ValidIssuer = sec.JwtIssuer,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sec.JwtSecurityKey)),
      ClockSkew = TimeSpan.Zero
    )

  let mongoDbSettings: DatabaseSettings =
    { connectionString = configuration.GetConnectionString("MongoDbConnection")
      databaseName = configuration.GetConnectionString("DatabaseName") }

  let mongoContext = MongoContext(mongoDbSettings) :> IMongoContext

  services
    .AddAuthentication(fun options ->
      options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
      options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme
      options.DefaultScheme <- JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(fun options ->
      options.SaveToken <- true
      options.RequireHttpsMetadata <- false
      options.TokenValidationParameters <- validationParameters)
  |> ignore


  let pol =
    (new AuthorizationPolicyBuilder())
      .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
      .RequireAuthenticatedUser()
      .Build()

  services.AddAuthorization(fun auth -> auth.AddPolicy("Bearer", pol)) |> ignore
  BsonSerializer.RegisterSerializer(GuidSerializer(GuidRepresentation.Standard))

  services.AddMemoryCache() |> ignore

  services
    .AddSingleton<IMongoContext>(mongoContext)
    // .AddTransient<IPaymentRepository, PaymentRepository>()
    // .AddScoped<UserSession>(fun sp -> UserSession.anon)
    .AddSingleton(sec)
    .AddSingleton<SessionManager>()
    .AddCors()
  |> ignore

  //events and services
  services.AddScoped<IUserRepository, UserRepository>() |> ignore

  services.AddScoped<IUserVerificationRepository, UserVerificationRepository>()
  |> ignore

  services.AddScoped<IProductTypeRepository, ProductTypeRepository>() |> ignore
  services.AddScoped<IProfessionalRepository, ProfessionalRepository>() |> ignore
  services.AddScoped<IProductRepository, ProductRepository>() |> ignore
  services.AddScoped<ISampleRepository, SampleRepository>() |> ignore
  services.AddScoped<ProfessionalQuery>() |> ignore
