@echo off

if "%~4" == "" goto :ERROR_REF

nuget-push-package.bat %1 %2 %3 %4
goto SUCCESS

:ERROR_REF
echo Usage: <script> distribution version source api-token
echo Execution has been aborted.

:SUCCESS