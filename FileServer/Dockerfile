FROM mcr.microsoft.com/dotnet/aspnet:8.0.14-alpine3.21 AS base
ENV ASPNETCORE_URLS=""
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV FileServer_SettingsFilePath="/app/settings/appsettings.json"
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.407-alpine3.21 AS build
WORKDIR /src
COPY ["FileServer/FileServer.csproj", "FileServer/"]
RUN dotnet restore "FileServer/FileServer.csproj"
COPY ["FileServer.Tests/FileServer.Tests.csproj", "FileServer.Tests/"]
RUN dotnet restore "FileServer.Tests/FileServer.Tests.csproj"
COPY . .
RUN dotnet build "FileServer/FileServer.csproj" -c Release --no-restore
RUN dotnet build "FileServer.Tests/FileServer.Tests.csproj" -c Release --no-restore
RUN dotnet test "FileServer.Tests/FileServer.Tests.csproj" -c Release --no-build

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

FROM build AS publish
COPY --from=e2etests /src/commands.txt /src/not_used.txt
RUN dotnet publish "FileServer/FileServer.csproj" -c Release --no-build -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileServer.dll"]
