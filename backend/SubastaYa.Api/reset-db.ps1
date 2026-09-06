# Ejecutar desde backend/SubastaYa.Api con: .\reset-db.ps1
# Borra la migración y la base de datos existentes, y las regenera con
# fechas de seed relativas al momento actual (para que las subastas
# "activas" de prueba realmente estén activas al arrancar).

Write-Host "Borrando migraciones y base de datos anteriores..."
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue .\Migrations
Remove-Item -Force -ErrorAction SilentlyContinue .\subastaya.db, .\subastaya.db-shm, .\subastaya.db-wal

Write-Host "Generando nueva migracion..."
dotnet ef migrations add InitialCreate

Write-Host "Aplicando migracion..."
dotnet ef database update

Write-Host "Listo. Ahora podes correr: dotnet run"
