using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.DTOs;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/wallet")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    // GET /api/v1/wallet/balance?usuarioId=2
    [HttpGet("balance")]
    public async Task<ActionResult<SaldoResponse>> ObtenerSaldo([FromQuery] int usuarioId)
    {
        var saldo = await _walletService.ObtenerSaldoAsync(usuarioId);
        return Ok(saldo);
    }

    // POST /api/v1/wallet/deposits  (carga de saldo simulada)
    [HttpPost("deposits")]
    public async Task<ActionResult<SaldoResponse>> Depositar([FromBody] DepositoRequest request)
    {
        var saldo = await _walletService.DepositarAsync(request);
        return Ok(saldo);
    }

    // GET /api/v1/wallet/movements?usuarioId=2
    [HttpGet("movements")]
    public async Task<ActionResult<List<MovimientoResponse>>> ObtenerMovimientos([FromQuery] int usuarioId)
    {
        var movimientos = await _walletService.ObtenerMovimientosAsync(usuarioId);
        return Ok(movimientos);
    }
}
