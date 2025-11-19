@echo off
pushd "%~dp0"

set "FileServer_SettingsFilePath=%cd%\FileServer\bin\settings\appsettings.json"
set "FileServer__Settings__ListenAddress=127.0.0.1"
set "FileServer__Settings__CertFilePath=%cd%\FileServer\bin\settings\cert.crt"
set "FileServer__Settings__CertKeyPath=%cd%\FileServer\bin\settings\cert.key"
set "FileServer__Settings__DownloadAnonDir=%cd%\FileServer\bin\fs_data\download_anon"
set "FileServer__Settings__DownloadDir=%cd%\FileServer\bin\fs_data\download"
set "FileServer__Settings__UploadDir=%cd%\FileServer\bin\fs_data\upload"

echo -^> Publishing...
if exist "FileServer\bin\publish" (rmdir "FileServer\bin\publish" /S /Q)>nul 2>&1
dotnet publish "FileServer/FileServer.csproj" -o "FileServer/bin/publish" -p:UseAppHost=false -v q
if %errorlevel% neq 0 goto :Error

echo -^> Running...
pushd "%cd%\FileServer\bin\publish"
call dotnet "FileServer.dll"
if %errorlevel% neq 0 goto :Error

:Success
echo =^> Completed successfully
goto :End

:Error
echo;
echo Press any key to close...
pause>nul

:End
popd
popd
