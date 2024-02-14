# Buduj aplikację
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Skopiuj pliki projektu i zbuduj aplikację
COPY . ./
RUN dotnet publish -c Release -o out

# Inicjalizuj bazę danych
FROM mcr.microsoft.com/mssql/server:2019-latest AS db-init
WORKDIR /db

COPY ./sql-server-init.bat /docker-entrypoint-initdb.d/sql-server-init.bat
RUN powershell -Command "(Get-Content -Path 'C:\Users\hp\Desktop\CloudCompare\PortalOgloszeniowy\sql-server-init.bat' -Raw) -replace '\r\n', '\n' | Set-Content -Path 'C:\Users\hp\Desktop\CloudCompare\PortalOgloszeniowy\sql-server-init.bat'"

# Buduj obraz końcowy
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "PortalOgloszeniowy.dll"]
