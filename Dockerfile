FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS sdk

FROM sdk AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY PowerLinesDataService/PowerLinesDataService.csproj ./PowerLinesDataService/
RUN dotnet restore ./PowerLinesDataService
COPY . .
RUN dotnet publish ./PowerLinesDataService -c Release --no-restore -o /out /p:UseAppHost=false

FROM sdk AS development
RUN apk add --no-cache curl procps unzip tzdata icu-libs \
    && wget -qO- https://aka.ms/getvsdbgsh | sh /dev/stdin -v latest -l /vsdbg \
    && addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet
WORKDIR /home/dotnet
COPY --chown=dotnet:dotnet . .
USER dotnet
ENV DOTNET_ENVIRONMENT=Development
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENTRYPOINT ["dotnet", "run", "--project", "PowerLinesDataService", "--no-launch-profile", "--"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS production
RUN apk add --no-cache tzdata icu-libs \
    && addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet
WORKDIR /home/dotnet
COPY --from=build --chown=dotnet:dotnet /out/ ./
USER dotnet
ENV DOTNET_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENTRYPOINT ["dotnet", "PowerLinesDataService.dll"]
