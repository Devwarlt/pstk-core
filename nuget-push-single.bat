@ECHO OFF

SETLOCAL
SET DISTRIBUTION=null
SET VERSION=null
SET SOURCE=null
SET TOKEN=null

ECHO Usage:
ECHO    Step 1) Enter package name: case sensitive and has same name of CSPROJ reference.
ECHO    Step 2) Enter package version: current version available in assembly reference.
ECHO    Step 3) Enter source: specify what is the deployment cloud on this package (nuget/github).
ECHO    Step 4) Enter API token: OAuth token used on deployment.
ECHO.

IF "%~1"=="" (SET /P VERSION="Enter package: ")
IF "%~2"=="" (SET /P VERSION="Enter version: ")
IF "%~3"=="" (SET /P SOURCE="Enter source: ")
IF "%~4"=="" (SET /P TOKEN="Enter API token: ")

ECHO.

nuget-push-package.bat %DISTRIBUTION% %VERSION% %SOURCE% %TOKEN%
ENDLOCAL