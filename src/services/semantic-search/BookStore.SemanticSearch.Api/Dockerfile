FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY src/shared/ src/shared/
COPY src/services/semantic-search/ src/services/semantic-search/

RUN dotnet restore src/services/semantic-search/BookStore.SemanticSearch.Api/BookStore.SemanticSearch.Api.csproj
RUN dotnet publish src/services/semantic-search/BookStore.SemanticSearch.Api/BookStore.SemanticSearch.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookStore.SemanticSearch.Api.dll"]
