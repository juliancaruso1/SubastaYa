using SubastaYa.Api.DTOs;

namespace SubastaYa.Api.Services;

public interface IWalletService
{
    Task<SaldoResponse> ObtenerSaldoAsync(int usuarioId);
    Task<SaldoResponse> DepositarAsync(DepositoRequest request);
    Task<List<MovimientoResponse>> ObtenerMovimientosAsync(int usuarioId);
}
