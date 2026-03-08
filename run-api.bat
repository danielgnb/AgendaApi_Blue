@echo off
setlocal

cd /d "%~dp0"

echo [1/2] Restaurando pacotes...
dotnet restore
if errorlevel 1 (
  echo Falha no restore.
  exit /b 1
)

echo [2/2] Iniciando API...
dotnet run --project ".\AgendaApi_Blue\AgendaApi_Blue.csproj"

endlocal
