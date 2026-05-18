FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY src/shared/ src/shared/
COPY src/services/catalog/ src/services/catalog/

RUN dotnet restore src/services/catalog/BookStore.Catalog.Api/BookStore.Catalog.Api.csproj
RUN dotnet publish src/services/catalog/BookStore.Catalog.Api/BookStore.Catalog.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookStore.Catalog.Api.dll"]
