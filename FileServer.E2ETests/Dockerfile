FROM mcr.microsoft.com/playwright/dotnet:v1.51.0-noble AS e2etests
WORKDIR /src
COPY ["FileServer/FileServer.csproj", "FileServer/"]
RUN dotnet restore "FileServer/FileServer.csproj"
COPY ["FileServer.E2ETests/FileServer.E2ETests.csproj", "FileServer.E2ETests/"]
RUN dotnet restore "FileServer.E2ETests/FileServer.E2ETests.csproj"
COPY . .
RUN dotnet build "FileServer/FileServer.csproj" -c Release --no-restore
RUN dotnet build "FileServer.E2ETests/FileServer.E2ETests.csproj" -c Release --no-restore
RUN dotnet test "FileServer.E2ETests/FileServer.E2ETests.csproj" -c Release --no-build
