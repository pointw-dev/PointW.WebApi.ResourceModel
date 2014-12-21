@echo off
setlocal

set nugethost=%computername%
if not x%1==x set nugethost=%1

del *.nupkg /q >nul 2>nul

for %%i in (*.csproj) do call :BuildPackage %%i
for %%i in (..\PointW.WebApi.MediaTypeFormatters.Hal\*.csproj) do call :BuildPackage %%i
for %%i in (..\PointW.WebApi.MediaTypeFormatters.CollectionJson\*.csproj) do call :BuildPackage %%i
for %%i in (*.csproj) do nuget pack %%i -IncludeReferencedProjects -Prop Configuration=Release

for %%i in (*.csproj) do nuget pack %%i -IncludeReferencedProjects -Prop Configuration=Release
for %%j in (*.nupkg) do nuget push "%%j" -s http://%nugethost%/apparch/
goto end

:BuildPackage
  msbuild %~f1 -p:Configuration=Release
GOTO :EOF
  
:end
endlocal