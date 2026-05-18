FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY src/gateway/BookStore.Gateway/ src/gateway/BookStore.Gateway/

RUN dotnet restore src/gateway/BookStore.Gateway/BookStore.Gateway.csproj
RUN dotnet publish src/gateway/BookStore.Gateway/BookStore.Gateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookStore.Gateway.dll"]
