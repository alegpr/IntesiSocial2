namespace social_V0._0._1.Services
{
    //
    // Servizio Scoped che mantiene lo stato dell'utente loggato all'interno
    // del circuito SignalR di Blazor Server.
    // Registrato come <c>AddScoped&lt;ISessionService, SessionService&gt;()</c>:
    // ogni connessione SignalR (tab/circuito) ha la propria istanza isolata,
    // garantendo che utenti diversi non condividano accidentalmente la sessione.
    // Quando lo stato cambia (login/logout), notifica i componenti sottoscritti
    // tramite l'evento .
    //
    public class SessionService : ISessionService
    {
        // Utente correntemente loggato. Null se nessuno è autenticato.
        public Utente? UtenteLoggato { get; private set; }

        //
        // Evento notificato a ogni cambiamento di stato (login/logout).
        // I componenti UI come <c>UserMenu</c> si sottoscrivono in <c>OnInitialized</c>
        // e chiamano <c>StateHasChanged()</c> per aggiornare la visualizzazione.
        //
        public event Action? OnChange;

        //
        // Ora corrente nel fuso orario italiano W. Europe Standard Time (UTC+1/+2).
        // Usata per visualizzare date e orari nei post senza dover convertire
        // manualmente in ogni pagina.
        //
        public DateTime OraAttuale
        {
            get
            {
                var fusoItaliano = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, fusoItaliano);
            }
        }

        //
        // Imposta l'utente come loggato e notifica i listener di .
        // Chiamato da <c>Login.razor</c> dopo la verifica delle credenziali.
        //
        public void Login(Utente utente)
        {
            UtenteLoggato = utente;
            NotifyStateChanged();
        }

        //
        // Resetta lo stato: imposta UtenteLoggato = null e notifica i listener.
        // Chiamato da <c>Logout.razor</c> quando l'utente clicca "Esci".
        //
        public void Logout()
        {
            UtenteLoggato = null;
            NotifyStateChanged();
        }

        //
        // Innesca l'evento  per tutti i sottoscrittori.
        // I delegati null-safe (?.) garantiscono che non venga sollevata
        // eccezione se nessun componente è sottoscritto.
        //
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}