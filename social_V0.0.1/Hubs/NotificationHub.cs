using Microsoft.AspNetCore.SignalR;

namespace social_V0._0._1.Hubs;

//
// Hub SignalR per la notifica real-time di eventi social.
//
// Flusso:
//   PostService.ToggleLikeAsync / InsertPostAsync chiamano
//   IHubContext<NotificationHub> per inviare un evento a TUTTI
//   i client connessi ("LikeChanged" o "NewPost").
//   Home.razor ascolta questi eventi via HubConnection e ricarica
//   i dati quando necessario (senza polling).
//
// I metodi qui definiti (es. JoinGroup) sono chiamabili DAL client
// se serve filtrare notifiche per utente; al momento usiamo
// Clients.All per broadcast globale.
//
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
