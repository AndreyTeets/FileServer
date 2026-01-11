FROM mcr.microsoft.com/playwright/dotnet:v1.56.0-noble AS e2etests-base
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --install-dir /usr/share/dotnet --channel 10.0
RUN apt-get update && apt-get install -y --no-install-recommends clang cmake build-essential zlib1g-dev && rm -rf /var/lib/apt/lists/*
WORKDIR /repo
COPY ["Directory.Build.props", "global.json", ".editorconfig", ".globalconfig", "./"]

FROM e2etests-base AS app-publish
COPY ["src/FileServer/", "src/FileServer/"]
RUN dotnet publish src/FileServer -o /app/publish -p:FsPublishAot=true

FROM e2etests-base AS e2etests
COPY ["tests/Directory.Build.props", "tests/"]
COPY ["tests/FileServer.E2ETests/", "tests/FileServer.E2ETests/"]
RUN dotnet build tests/FileServer.E2ETests -c Release
COPY --from=app-publish /app/publish/ artifacts/publish-e2etests/
COPY ["src/FileServer/appsettings.template.json", "src/FileServer/"]

ARG FS_E2ETESTS_USE_PUBLISHED_APP="1"
ARG BROWSER="chromium"
RUN dotnet test --project tests/FileServer.E2ETests -c Release --no-build
