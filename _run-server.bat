@echo off
pushd "%~dp0"

set "FileServer_SettingsFilePath=%cd%\src\FileServer\bin\settings\appsettings.json"
set "FileServer__Settings__ListenAddress=127.0.0.1"
set "FileServer__Settings__CertFilePath=%cd%\src\FileServer\bin\settings\cert.crt"
set "FileServer__Settings__CertKeyPath=%cd%\src\FileServer\bin\settings\cert.key"
set "FileServer__Settings__DownloadAnonDir=%cd%\src\FileServer\bin\fs_data\anonymous_downloads"
set "FileServer__Settings__DownloadDir=%cd%\src\FileServer\bin\fs_data\downloads"
set "FileServer__Settings__UploadDir=%cd%\src\FileServer\bin\fs_data\uploads"

echo -^> Publishing...
if exist "artifacts\publish-dev" (rmdir "artifacts\publish-dev" /S /Q)>nul 2>&1
dotnet publish src/FileServer -o artifacts/publish-dev -p:UseAppHost=false -v q
if %errorlevel% neq 0 goto :Error

echo -^> Running...
pushd "artifacts\publish-dev"
call dotnet FileServer.dll
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
