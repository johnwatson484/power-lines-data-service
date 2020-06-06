# Development
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS development

RUN apk update \
  && apk --no-cache add curl procps unzip \
  && wget -qO- https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

RUN addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

USER dotnet
WORKDIR /home/dotnet

RUN mkdir -p /home/dotnet/PowerLinesDataService/ /home/dotnet/PowerLinesDataService.Tests/
COPY --chown=dotnet:dotnet ./PowerLinesDataService.Tests/*.csproj ./PowerLinesDataService.Tests/
RUN dotnet restore ./PowerLinesDataService.Tests/PowerLinesDataService.Tests.csproj
COPY --chown=dotnet:dotnet ./PowerLinesDataService/*.csproj ./PowerLinesDataService/
RUN dotnet restore ./PowerLinesDataService/PowerLinesDataService.csproj
COPY --chown=dotnet:dotnet ./PowerLinesDataService.Tests/ ./PowerLinesDataService.Tests/
RUN true
COPY --chown=dotnet:dotnet ./PowerLinesDataService/ ./PowerLinesDataService/
RUN dotnet publish ./PowerLinesDataService/ -c Release -o /home/dotnet/out

ENV ASPNETCORE_ENVIRONMENT=development
# Override entrypoint using shell form so that environment variables are picked up
ENTRYPOINT dotnet watch --project ./PowerLinesDataService run

# Production
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS production

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

USER dotnet
WORKDIR /home/dotnet

COPY --from=development /home/dotnet/out/ ./
ENV ASPNETCORE_ENVIRONMENT=production
# Override entrypoint using shell form so that environment variables are picked up
ENTRYPOINT dotnet PowerLinesDataService.dll
