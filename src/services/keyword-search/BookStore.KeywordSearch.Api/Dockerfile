FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY src/shared/ src/shared/
COPY src/services/keyword-search/ src/services/keyword-search/

RUN dotnet restore src/services/keyword-search/BookStore.KeywordSearch.Api/BookStore.KeywordSearch.Api.csproj
RUN dotnet publish src/services/keyword-search/BookStore.KeywordSearch.Api/BookStore.KeywordSearch.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookStore.KeywordSearch.Api.dll"]
