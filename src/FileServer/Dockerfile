FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS dev-and-tests-runtime

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS sdk
WORKDIR /repo

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS app-base
WORKDIR /app
ENV ASPNETCORE_URLS=""
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV FileServer_SettingsFilePath="/app/settings/appsettings.json"
ENTRYPOINT ["./FileServer"]
CMD []

#===app-jit==={
FROM sdk AS build-jit
ARG TARGETARCH APP_VERSION
COPY . .
RUN dotnet publish src/FileServer -o /app/publish -r "linux-musl-$TARGETARCH" -p:PublishTrimmed=true

FROM app-base AS app-jit
COPY --from=build-jit /app/publish .
#}

#===app-aot==={
FROM sdk AS build-aot-sysroot
ARG TARGETARCH
COPY --from=app-base / /sysroot/
RUN if [ "$TARGETARCH" == "amd64" ]; then ALPINE_ARCH="x86_64"; \
    elif [ "$TARGETARCH" == "arm64" ]; then ALPINE_ARCH="aarch64"; \
    elif [ "$TARGETARCH" == "arm" ]; then ALPINE_ARCH="armv7"; \
    else exit 10; fi && \
    apk add --no-cache --root /sysroot --arch $ALPINE_ARCH build-base zlib-dev

FROM sdk AS build-aot
ARG TARGETARCH APP_VERSION
RUN apk add --no-cache clang make cmake lld llvm
COPY --from=build-aot-sysroot /sysroot/ /sysroot/

COPY . .
RUN dotnet publish src/FileServer -o /app/publish -r "linux-musl-$TARGETARCH" -p:FsPublishAot=true \
    -p:CppCompilerAndLinker=clang -p:LinkerFlavor=lld -p:SysRoot=/sysroot

FROM app-base AS app-aot
COPY --from=build-aot /app/publish .
#}

#===tests==={
FROM sdk AS tests
COPY --from=dev-and-tests-runtime /usr/share/dotnet/shared /usr/share/dotnet/shared

COPY ["Directory.Build.props", "global.json", ".editorconfig", "./"]
COPY ["src/FileServer/FileServer.csproj", "src/FileServer/"]
COPY ["tests/FileServer.Tests/FileServer.Tests.csproj", "tests/FileServer.Tests/"]
RUN dotnet restore tests/FileServer.Tests

COPY . .
RUN dotnet build tests/FileServer.Tests -c Release --no-restore
RUN dotnet test tests/FileServer.Tests -c Release --no-build
#}
