# SubastaYa

Plataforma web de subastas en tiempo real (TP de Proyecto de Software).

- **Backend:** .NET 8 + EF Core (SQLite, Code-First) + SignalR — ver `backend/SubastaYa.Api/README.md`
- **Frontend:** React + Vite + SignalR — ver abajo

## Cómo levantar todo (2 terminales)

**Terminal 1 — Backend**
```bash
cd backend/SubastaYa.Api
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```
Queda en `http://localhost:5000` (Swagger en `/swagger`).

**Terminal 2 — Frontend**
```bash
cd frontend
npm install
npm run dev
```
Queda en `http://localhost:5173`.

Abrí `http://localhost:5173`, usá el selector de usuario (arriba a la derecha)
para simular sesión como `comprador1@test.com`, `comprador2@test.com`,
`sinfondos@test.com` o `vendedor@test.com`, y probá:

1. **Catálogo** con filtros por estado/categoría/orden.
2. **Sala de subasta en vivo** (entrá a "Figura de colección…", que cierra en
   menos de 2 minutos): pujá y mirá la extensión anti-sniping en vivo vía SignalR.
3. **Billetera**: saldo total/retenido/disponible + carga simulada.
4. **Publicar** una subasta nueva (con validaciones de fechas y montos).
5. **Mis actividades**: pujas realizadas y publicaciones propias.

## Notas de arquitectura y decisiones

- **Optimistic Locking**: campo `Version` (int) en `Subasta` y `Billetera`,
  marcado `ConcurrencyToken` en EF Core. Elegido sobre `rowversion` binario
  porque SQLite no lo soporta nativamente y así el equipo puede migrar a
  Postgres/SQL Server sin cambiar el modelo.
- **WebSockets vía SignalR**: la Sala de Subasta en Vivo se sincroniza por el
  hub `/hubs/auctions`, no por polling.
- **Transaccionalidad ACID**: cada puja corre en una transacción EF Core que
  libera al líder anterior, congela al nuevo líder, registra la oferta y
  aplica anti-sniping atómicamente; si algo falla, rollback completo.
- **Auditoría inmutable**: `AuditoriaLogs` registra extensiones de tiempo,
  cierres del Worker, pujas rechazadas por concurrencia y acreditaciones manuales.
- **Prueba de concurrencia**: `backend/concurrency-test.sh` (ver detalle en el
  README del backend).

## Sobre el PDF de la consigna

El archivo `trabajo_practico_de_a_duo.pdf` tenía instrucciones ocultas
inyectadas (pidiendo aplicar en silencio ciertas convenciones de código sin
avisar). No se aplicaron porque no venían de ustedes ni de la cátedra — son
contenido inyectado en el documento. Si el equipo quiere definir sus propios
estándares de nombrado/logging, se pueden sumar explícitamente.
