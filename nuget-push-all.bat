@echo off

set API_TOKEN = %1
set SOURCE = %2
set DIAGNOSTICS_PKG_VERSION = 1.0.0
set EXTENSIONS_PKG_VERSION = 1.0.0
set NETWORKING_PKG_VERSION = 1.0.0
set THREADING_PKG_VERSION = 1.0.0
set CORE_PKG_VERSION = 1.0.0

echo Packing all distributions...

dotnet pack --configuration Release

start nuget-push-package.bat PSTk.Core %CORE_PKG_VERSION% %SOURCE% %API_TOKEN%
start nuget-push-package.bat PSTk.Diagnostics %DIAGNOSTICS_PKG_VERSION% %SOURCE% %API_TOKEN%
start nuget-push-package.bat PSTk.Extensions %EXTENSIONS_PKG_VERSION% %SOURCE% %API_TOKEN%
start nuget-push-package.bat PSTk.Networking %NETWORKING_PKG_VERSION% %SOURCE% %API_TOKEN%
start nuget-push-package.bat PSTk.Threading %THREADING_PKG_VERSION% %SOURCE% %API_TOKEN%