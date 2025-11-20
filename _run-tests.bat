@echo off
pushd "%~dp0"

echo -^> Building solution...
dotnet build -c Release -v q
if %errorlevel% neq 0 goto :Error

echo -^> Format verifying solution...
dotnet format --verify-no-changes --no-restore -v q
if %errorlevel% neq 0 goto :Error

echo -^> Running tests...
dotnet test tests/FileServer.Tests -c Release --no-build -v q
if %errorlevel% neq 0 goto :Error

echo -^> Installing Playwright...
pwsh tests/FileServer.E2ETests/bin/Release/net10.0/playwright.ps1 install --only-shell chromium
if %errorlevel% neq 0 goto :Error

echo -^> Running E2E tests...
dotnet test tests/FileServer.E2ETests -c Release --no-build -v q
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
