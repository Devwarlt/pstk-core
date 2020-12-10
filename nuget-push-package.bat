@echo off

IF "%~4" == "" goto ERROR_REF

set DISTRIBUTION = %1
set VERSION = %2
set SOURCE = %3
set TOKEN = %4

echo Preparing to deploy...
echo Distribution: '%DISTRIBUTION%'
echo Version: %VERSION%
ECHO Source: %SOURCE%

timeout 3

dotnet nuget push "%DISTRIBUTION%\bin\Release\%DISTRIBUTION%.%VERSION%.nupkg" --source "%SOURCE%" --api-key %TOKEN%
goto SUCCESS

:ERROR_REF
echo Usage: <script> distribution version source api-token
echo Unable to perform this operation.
goto SUCCESS

:SUCCESS