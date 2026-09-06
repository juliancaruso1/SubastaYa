using Microsoft.AspNetCore.SignalR;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Hubs;

// Cada cliente que abre la Sala de Subasta en Vivo se une a un grupo por subastaId.
// El servidor emite "PujaRegistrada" (nueva oferta / extensión anti-sniping) y
// "SubastaCerrada" (cuando el Worker en segundo plano finaliza la subasta)
// a todos los miembros de ese grupo.
public class AuctionHub : Hub
{
    public async Task UnirseASubasta(int subastaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AuctionService.GrupoSubasta(subastaId));
    }

    public async Task SalirDeSubasta(int subastaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, AuctionService.GrupoSubasta(subastaId));
    }
}
