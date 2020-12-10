@ECHO OFF

SETLOCAL
SET SOURCE=null
SET TOKEN=null

ECHO Usage:
ECHO    Step 1) Enter source: specify what is the deployment cloud on this package (nuget/github).
ECHO    Step 2) Enter API token: OAuth token used on deployment.
ECHO.

IF "%~1"=="" (SET /P SOURCE="Enter source: ")
IF "%~2"=="" (SET /P TOKEN="Enter API token: ")

ECHO.

SET CORE_PKG_VERSION=1.0.0
SET DIAGNOSTICS_PKG_VERSION=1.0.0
SET EXTENSIONS_PKG_VERSION=1.0.0
SET NETWORKING_PKG_VERSION=1.0.0
SET THREADING_PKG_VERSION=1.0.0

ECHO Packing all distributions...

ECHO dotnet pack --configuration Release

start nuget-push-package.bat PSTk.Core %CORE_PKG_VERSION% %SOURCE% %TOKEN%
start nuget-push-package.bat PSTk.Diagnostics %DIAGNOSTICS_PKG_VERSION% %SOURCE% %TOKEN%
start nuget-push-package.bat PSTk.Extensions %EXTENSIONS_PKG_VERSION% %SOURCE% %TOKEN%
start nuget-push-package.bat PSTk.Networking %NETWORKING_PKG_VERSION% %SOURCE% %TOKEN%
start nuget-push-package.bat PSTk.Threading %THREADING_PKG_VERSION% %SOURCE% %TOKEN%
ENDLOCAL