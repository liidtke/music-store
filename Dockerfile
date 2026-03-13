FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Store/Store.fsproj", "Store/"]
COPY ["Store.Domain/Store.Domain.fsproj", "Store.Domain/"]
COPY ["Store.Infrastructure/Store.Infrastructure.fsproj", "Store.Infrastructure/"]
RUN dotnet restore "Store/Store.fsproj"
COPY . .
WORKDIR "/src/Store"
RUN dotnet build "./Store.fsproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Store.fsproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.dll"]
