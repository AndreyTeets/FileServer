@echo off
pushd "%~dp0"

set "FileServer_SettingsFilePath=%cd%\FileServer\bin\settings\appsettings.json"
set "FileServer__Settings__ListenAddress=127.0.0.1"
set "FileServer__Settings__CertFilePath=%cd%\FileServer\bin\settings\cert.crt"
set "FileServer__Settings__CertKeyPath=%cd%\FileServer\bin\settings\cert.key"
set "FileServer__Settings__DownloadAnonDir=%cd%\FileServer\bin\fs_data\download_anon"
set "FileServer__Settings__DownloadDir=%cd%\FileServer\bin\fs_data\download"
set "FileServer__Settings__UploadDir=%cd%\FileServer\bin\fs_data\upload"

pushd "%cd%\FileServer\bin\publish"
call "FileServer.exe"
if %errorlevel% neq 0 goto :Error
goto :End

:Error
echo;
echo Press any key to close...
pause>nul
:End
popd
popd
