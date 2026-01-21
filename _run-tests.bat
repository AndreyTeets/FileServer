@echo off
pushd "%~dp0"

echo -^> Building solution...
dotnet build -c Release -v q
if %errorlevel% neq 0 goto :Error

echo -^> Format verifying solution...
set "FS_DISABLE_TRIM_ANALYZER=true"
dotnet format --verify-no-changes --no-restore -v q
set "FS_DISABLE_TRIM_ANALYZER="
if %errorlevel% neq 0 goto :Error

echo -^> Running tests...
dotnet test --project tests/FileServer.Tests -c Release --no-build -v q
if %errorlevel% neq 0 goto :Error

echo -^> Installing Playwright...
pwsh tests/FileServer.E2ETests/bin/Release/net10.0/playwright.ps1 install --only-shell chromium
if %errorlevel% neq 0 goto :Error

echo -^> Publishing and cleaning app project...
dotnet publish src/FileServer -o artifacts/publish-e2etests -c Release -v q ^
    -p:PublishTrimmed=true -p:FsPublishSingleFile=true -p:EnableCompressionInSingleFile=true
if %errorlevel% neq 0 goto :Error
dotnet clean src/FileServer -c Release -v q
if %errorlevel% neq 0 goto :Error

echo -^> Running E2E tests...
set "FS_E2ETESTS_USE_PUBLISHED_APP=1"
set "BROWSER=chromium"
dotnet test --project tests/FileServer.E2ETests -c Release --no-build -v q
if %errorlevel% neq 0 goto :Error

:Success
echo =^> All tests completed successfully
goto :End

:Error

:End
echo;
echo Press any key to close...
pause>nul
popd
