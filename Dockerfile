# BASE
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=production

# DEVELOPMENT
FROM base AS development-env
WORKDIR /PowerLinesDataService
RUN apt-get update \
 && apt-get install -y --no-install-recommends unzip \
 && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
COPY ./PowerLinesDataService/*.csproj ./
RUN dotnet restore
COPY ./PowerLinesDataService ./
ENTRYPOINT [ "dotnet", "watch", "run" ]

# TEST
FROM development-env AS test-env
WORKDIR /PowerLinesDataService.Tests
COPY ./PowerLinesDataService.Tests/*.csproj ./
RUN dotnet restore
COPY ./PowerLinesDataService.Tests ./
ENTRYPOINT [ "dotnet", "test" ]

# PRODUCTION
FROM base AS build-env
COPY ./PowerLinesDataService/*.csproj ./
RUN dotnet restore
COPY ./PowerLinesDataService ./
RUN dotnet publish -c Release -o out

# RUNTIME
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS production-env
WORKDIR /app
COPY --from=build-env /app/out .
RUN chown -R www-data:www-data /app
USER www-data
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PowerLinesDataService.dll"]
