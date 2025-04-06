FROM mcr.microsoft.com/dotnet/aspnet:8.0.14-alpine3.21 AS base

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

FROM build AS publish
RUN dotnet publish "FileServer/FileServer.csproj" -c Release --no-build -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=""
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV FileServer_SettingsFilePath="/app/settings/appsettings.json"
ENTRYPOINT ["dotnet", "FileServer.dll"]
CMD []
