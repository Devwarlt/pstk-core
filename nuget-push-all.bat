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

SET CORE_PKG_VERSION=1.1.2
SET DIAGNOSTICS_PKG_VERSION=1.2.0
SET EXTENSIONS_PKG_VERSION=1.2.0
SET NETWORKING_PKG_VERSION=1.1.0
SET REDIS_PKG_VERSION=1.0.1
SET THREADING_PKG_VERSION=1.3.0

ECHO Packing all distributions...

dotnet pack --configuration Release

START nuget-push-package.bat PSTk.Core %CORE_PKG_VERSION% %SOURCE% %TOKEN%
START nuget-push-package.bat PSTk.Diagnostics %DIAGNOSTICS_PKG_VERSION% %SOURCE% %TOKEN%
START nuget-push-package.bat PSTk.Extensions %EXTENSIONS_PKG_VERSION% %SOURCE% %TOKEN%
START nuget-push-package.bat PSTk.Networking %NETWORKING_PKG_VERSION% %SOURCE% %TOKEN%
START nuget-push-package.bat PSTk.Threading %THREADING_PKG_VERSION% %SOURCE% %TOKEN%
START nuget-push-package.bat PSTk.Redis %REDIS_PKG_VERSION% %SOURCE% %TOKEN%
ENDLOCAL