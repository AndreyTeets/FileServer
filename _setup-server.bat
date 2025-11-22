@echo off
pushd "%~dp0"

set "SETTINGS_DIR=src\FileServer\bin\settings"
set "FS_DATA_DIR=src\FileServer\bin\fs_data"

:CopyAppSettingsJsonFromTemplate
if exist "%SETTINGS_DIR%\appsettings.json" goto :SetUpFsDataDirectory
echo -^> Copying appsettings.json from template...
echo F| xcopy "src\FileServer\appsettings.template.json" "%SETTINGS_DIR%\appsettings.json" /Y /R /Q>nul
echo Don't forget to manually set SigningKey/LoginKey

:SetUpFsDataDirectory
if exist "%FS_DATA_DIR%" goto :GenerateCertificate
echo -^> Setting up fs_data directory...
(mkdir "%FS_DATA_DIR%\anonymous_downloads")>nul 2>&1 && echo test_anonfile1_content> "%FS_DATA_DIR%\anonymous_downloads\anonfile1.txt"
(mkdir "%FS_DATA_DIR%\downloads")>nul 2>&1 && echo test_file1_content> "%FS_DATA_DIR%\downloads\file1.txt"
(mkdir "%FS_DATA_DIR%\uploads")>nul 2>&1

:GenerateCertificate
if exist "%SETTINGS_DIR%\cert.crt" goto :Success
echo -^> Generating certificate...
openssl req -quiet -x509 -newkey rsa:4096 -sha256 -days 365 -nodes -keyout "%SETTINGS_DIR%/cert.key" -out "%SETTINGS_DIR%/cert.crt" -subj "/CN=localhost"
if %errorlevel% neq 0 goto :Error

:Success
echo =^> Setup completed successfully
goto :End

:Error

:End
echo;
echo Press any key to close...
pause>nul
popd
