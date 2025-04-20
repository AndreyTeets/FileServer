FROM mcr.microsoft.com/playwright/dotnet:v1.51.0-noble AS e2etests
WORKDIR /src
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --install-dir /usr/share/dotnet --channel 9.0

COPY ["Directory.Build.props", "global.json", ".editorconfig", "./"]
COPY ["FileServer/FileServer.csproj", "FileServer/"]
RUN dotnet restore "FileServer/FileServer.csproj"
COPY ["FileServer.E2ETests/FileServer.E2ETests.csproj", "FileServer.E2ETests/"]
RUN dotnet restore "FileServer.E2ETests/FileServer.E2ETests.csproj"

COPY ["FileServer/", "FileServer/"]
RUN dotnet build "FileServer/FileServer.csproj" -c Release --no-restore
COPY ["FileServer.E2ETests/", "FileServer.E2ETests/"]
RUN dotnet build "FileServer.E2ETests/FileServer.E2ETests.csproj" -c Release --no-restore
RUN dotnet format "FileServer.E2ETests/FileServer.E2ETests.csproj" --verify-no-changes --no-restore
RUN dotnet test "FileServer.E2ETests/FileServer.E2ETests.csproj" -c Release --no-build
