@echo off

REM Czekaj aż SQL Server zostanie uruchomiony
echo Waiting for SQL Server to be available...
timeout /t 30 >nul

REM Wykonaj migracje za pomocą narzędzia dotnet ef
echo Running migrations...
dotnet ef database update

echo Migrations completed.
