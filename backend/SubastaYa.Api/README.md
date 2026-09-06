# SubastaYa — Backend (.NET 8 + EF Core + SignalR)

## Requisitos
- .NET SDK 8.0+
- (Usa SQLite por defecto, no requiere instalar un motor de base de datos aparte)

## Puesta en marcha

```bash
cd backend/SubastaYa.Api
dotnet restore

# Genera la primera migración a partir de los modelos (Code-First)
dotnet ef migrations add InitialCreate

# Aplica la migración y crea subastaya.db (además, Program.cs también
# llama a Database.Migrate() automáticamente al arrancar)
dotnet ef database update

dotnet run
```

La API queda en `https://localhost:5001` (o el puerto que indique la consola) con:
- Swagger UI en `/swagger`
- Hub de SignalR en `/hubs/auctions`

> Si no tenés `dotnet-ef` instalado: `dotnet tool install --global dotnet-ef`

## Datos semilla

Al aplicar las migraciones se cargan automáticamente (vía `HasData`):
- 4 usuarios + billeteras (`vendedor@test.com`, `comprador1@test.com`, `comprador2@test.com`, `sinfondos@test.com`)
- 4 categorías
- 5 subastas cubriendo los 5 casos de prueba pedidos (activa estándar, activa crítica, próxima, vencida con ganador, vencida desierta)
- Historial de pujas y transacciones de ledger consistentes con esos escenarios

## Arquitectura

```
Controllers/   -> capa HTTP (solo reciben requests y delegan)
Services/      -> lógica de negocio y transaccionalidad (AuctionService, WalletService, AuditService)
Data/          -> AppDbContext + configuración de EF Core
Models/        -> entidades del dominio
DTOs/          -> contratos de entrada/salida de la API
Hubs/          -> AuctionHub (SignalR) para la sala de subasta en vivo
Workers/       -> AuctionClosingWorker (BackgroundService que adjudica subastas vencidas)
Middleware/    -> traducción de excepciones de dominio a códigos HTTP
```

### Concurrencia optimista

`Subasta.Version` y `Billetera.Version` están marcados como `ConcurrencyToken` en
`AppDbContext`. Cada vez que se escribe sobre una subasta o billetera, EF Core
compara el valor de `Version` que tenía en memoria contra el que hay en la base;
si otro proceso ya la modificó, lanza `DbUpdateConcurrencyException`, que el
`AuctionService`/`WalletService` traducen a `409 Conflict` (ver
`ConflictoConcurrenciaException` + `ExceptionHandlingMiddleware`).

Además, `POST /api/v1/auctions/{id}/bids` recibe explícitamente la
`VersionEsperada` que el cliente tenía al momento de ofertar, para detectar el
conflicto **antes** incluso de tocar la base.

### Prueba de Concurrencia (Stress Test)

El script `concurrency-test.sh` envía **dos pujas idénticas en simultáneo**
sobre la subasta activa estándar (Id 1) usando `curl` en background, para
demostrar que la base registra una sola y rechaza la otra con `409 Conflict`:

```bash
cd backend
chmod +x concurrency-test.sh
./concurrency-test.sh
```

Salida esperada: una respuesta `200 OK` y una `409 Conflict`.

#### Evidencia real de ejecución

Corrido el `2026-09-05` contra la subasta semilla Id 1 (Notebook Gamer RTX,
puja actual $45.000, incremento mínimo $1.000, `VersionEsperada=2`):

```
Disparando 2 pujas idénticas en paralelo contra http://localhost:5000/api/v1/auctions/1/bids ...
Request 1 -> HTTP 200
Request 2 -> HTTP 409

--- Respuesta 1 ---
{"extendida":false,"nuevaFechaFin":"2026-09-05T23:17:03.7674061Z",
 "subasta":{"id":1,"titulo":"Notebook Gamer RTX","pujaActual":46000,
 "proximaPujaSugerida":47000.0,"estado":"Activa","version":3, ... }}

--- Respuesta 2 ---
{"Mensaje":"La subasta cambió de estado desde que la consultaste. Refrescá e intentá de nuevo."}

Se espera un 200 OK y un 409 Conflict.
```

De las dos pujas idénticas enviadas en simultáneo, la base de datos aceptó
una sola (`Version` pasó de `2` a `3`, `pujaActual` quedó en `$46.000`) y
rechazó la otra con `409 Conflict`, confirmando que la concurrencia
optimista funciona como se diseñó.

## Reglas de negocio implementadas

- **Escrow atómico**: al pujar, se libera la retención del líder anterior y se
  congela el saldo del nuevo líder dentro de la misma transacción de base de datos.
- **Anti-sniping**: si una puja válida llega dentro de los últimos 60 segundos,
  la subasta extiende su cierre 2 minutos (configurable en `appsettings.json`).
- **Worker en segundo plano**: cada 10 segundos (configurable) busca subastas
  vencidas, liquida a comprador/vendedor si hubo ganador, o las pasa a `DESIERTA`
  si no hubo pujas.
- **Auditoría inmutable**: extensiones de tiempo, cierres del Worker, pujas
  rechazadas por concurrencia y acreditaciones manuales quedan en `AuditoriaLogs`.
