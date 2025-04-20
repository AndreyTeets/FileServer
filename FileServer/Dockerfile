FROM mcr.microsoft.com/dotnet/aspnet:8.0.14-alpine3.21 AS app-base

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0.203-alpine3.21 AS build
ARG TARGETARCH APP_VERSION
WORKDIR /src
COPY ["Directory.Build.props", "global.json", ".editorconfig", "./"]
COPY ["FileServer/FileServer.csproj", "FileServer/"]
RUN dotnet restore "FileServer/FileServer.csproj" -a $TARGETARCH
COPY ["FileServer.Tests/FileServer.Tests.csproj", "FileServer.Tests/"]
RUN dotnet restore "FileServer.Tests/FileServer.Tests.csproj" -a $TARGETARCH
COPY ["FileServer/", "FileServer/"]
RUN dotnet build "FileServer/FileServer.csproj" -a $TARGETARCH -c Release --no-restore
RUN dotnet format "FileServer/FileServer.csproj" --verify-no-changes --no-restore

FROM build AS publish
ARG TARGETARCH APP_VERSION
RUN dotnet publish "FileServer/FileServer.csproj" -a $TARGETARCH -c Release --no-build -o /app/publish -p:UseAppHost=false

FROM build AS tests
ARG TARGETARCH APP_VERSION
COPY ["FileServer.Tests/", "FileServer.Tests/"]
RUN dotnet build "FileServer.Tests/FileServer.Tests.csproj" -a $TARGETARCH -c Release --no-restore
RUN dotnet format "FileServer.Tests/FileServer.Tests.csproj" --verify-no-changes --no-restore
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
