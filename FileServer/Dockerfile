FROM mcr.microsoft.com/dotnet/aspnet:10.0.0-alpine3.22 AS app-base

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.100-alpine3.22 AS build
ARG TARGETARCH APP_VERSION
WORKDIR /src
COPY ["Directory.Build.props", "global.json", ".editorconfig", "./"]
COPY ["FileServer/FileServer.csproj", "FileServer/"]
RUN dotnet restore "FileServer/FileServer.csproj" -a $TARGETARCH
COPY ["FileServer.Tests/FileServer.Tests.csproj", "FileServer.Tests/"]
RUN dotnet restore "FileServer.Tests/FileServer.Tests.csproj" -a $TARGETARCH
COPY ["FileServer/", "FileServer/"]
RUN dotnet build "FileServer/FileServer.csproj" -a $TARGETARCH -c Release --no-restore

FROM build AS publish
ARG TARGETARCH APP_VERSION
RUN dotnet publish "FileServer/FileServer.csproj" -o /app/publish -a $TARGETARCH -p:UseAppHost=false --no-build

FROM build AS tests
ARG TARGETARCH APP_VERSION
COPY ["FileServer.Tests/", "FileServer.Tests/"]
RUN dotnet build "FileServer.Tests/FileServer.Tests.csproj" -a $TARGETARCH -c Release --no-restore
COPY --from=app-base /usr/share/dotnet/shared /usr/share/dotnet/shared
RUN dotnet test "FileServer.Tests/FileServer.Tests.csproj" -a $TARGETARCH -c Release --no-build

FROM app-base AS app
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=""
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV FileServer_SettingsFilePath="/app/settings/appsettings.json"
ENTRYPOINT ["dotnet", "FileServer.dll"]
CMD []
