FROM mcr.microsoft.com/playwright/dotnet:v1.56.0-noble AS e2etests
WORKDIR /repo
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --install-dir /usr/share/dotnet --channel 10.0

COPY ["Directory.Build.props", "global.json", ".editorconfig", "./"]
COPY ["src/FileServer/FileServer.csproj", "src/FileServer/"]
RUN dotnet restore src/FileServer
COPY ["tests/FileServer.E2ETests/FileServer.E2ETests.csproj", "tests/FileServer.E2ETests/"]
RUN dotnet restore tests/FileServer.E2ETests

COPY ["src/FileServer/", "src/FileServer/"]
RUN dotnet build src/FileServer -c Release --no-restore
COPY ["tests/FileServer.E2ETests/", "tests/FileServer.E2ETests/"]
RUN dotnet build tests/FileServer.E2ETests -c Release --no-restore
RUN dotnet test tests/FileServer.E2ETests -c Release --no-build
