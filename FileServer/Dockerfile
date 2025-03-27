FROM mcr.microsoft.com/dotnet/aspnet:8.0.14-alpine3.21 AS base
ENV ASPNETCORE_URLS=""
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV FileServer_SettingsFilePath="/app/settings/appsettings.json"
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.407-alpine3.21 AS build
WORKDIR /src
COPY ["FileServer/FileServer.csproj", "FileServer/"]
RUN dotnet restore "FileServer/FileServer.csproj"
COPY . .
WORKDIR "/src/FileServer"
RUN dotnet build "FileServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileServer.dll"]
