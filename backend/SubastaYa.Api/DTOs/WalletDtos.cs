using SubastaYa.Api.Enums;

namespace SubastaYa.Api.DTOs;

public record SaldoResponse(
    decimal SaldoTotal,
    decimal SaldoRetenido,
    decimal SaldoDisponible
);

public record DepositoRequest(
    int UsuarioId,
    decimal Monto
);

public record MovimientoResponse(
    TipoTransaccion Tipo,
    decimal Monto,
    DateTime Fecha,
    int? SubastaId
);

public record ErrorResponse(string Mensaje);
