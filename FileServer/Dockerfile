FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0.0-alpine3.22 AS dev-and-tests-runtime

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.100-alpine3.22 AS sdk
WORKDIR /src

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0.0-alpine3.22 AS app-base
WORKDIR /app
ENV ASPNETCORE_URLS=""
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV FileServer_SettingsFilePath="/app/settings/appsettings.json"
ENTRYPOINT ["./FileServer"]
CMD []

#===app==={
FROM sdk AS build
ARG TARGETARCH APP_VERSION
COPY . .
RUN dotnet publish "FileServer/FileServer.csproj" -o /app/publish -r "linux-musl-$TARGETARCH" -p:PublishTrimmed=true

FROM app-base AS app
COPY --from=build /app/publish .
#}

#===tests==={
FROM sdk AS tests
COPY --from=dev-and-tests-runtime /usr/share/dotnet/shared /usr/share/dotnet/shared

COPY ["Directory.Build.props", "global.json", ".editorconfig", "./"]
COPY ["FileServer/FileServer.csproj", "FileServer/"]
COPY ["FileServer.Tests/FileServer.Tests.csproj", "FileServer.Tests/"]
RUN dotnet restore "FileServer.Tests/FileServer.Tests.csproj"

COPY . .
RUN dotnet build "FileServer.Tests/FileServer.Tests.csproj" -c Release --no-restore
RUN dotnet test "FileServer.Tests/FileServer.Tests.csproj" -c Release --no-build
#}
