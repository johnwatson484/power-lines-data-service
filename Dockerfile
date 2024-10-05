# Development
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS development

RUN apk update \
  && apk --no-cache add curl procps unzip \
  && wget -qO- https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

RUN addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

USER dotnet
WORKDIR /home/dotnet

COPY --chown=dotnet:dotnet . .

RUN dotnet publish ./PowerLinesDataService/ -c Release -o /home/dotnet/out

ENV ASPNETCORE_ENVIRONMENT=development

ENTRYPOINT ["dotnet", "watch", "--project", "./PowerLinesDataService", "run"]

# Production
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS production

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

USER dotnet
WORKDIR /home/dotnet

COPY --from=development /home/dotnet/out/ ./
ENV ASPNETCORE_ENVIRONMENT=production

ENTRYPOINT ["dotnet", "PowerLinesDataService.dll"]
