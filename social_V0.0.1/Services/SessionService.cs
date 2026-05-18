using social_V0._0._1.Models;

namespace social_V0._0._1.Services
{
// Servizio Singleton: mantiene l'utente loggato tra le pagine e notifica i componenti via OnChange.
    public class SessionService
    {
        public Utente? UtenteLoggato { get; private set; }
        public event Action? OnChange;

// Ora corrente nel fuso italiano (W. Europe Standard Time).
        public DateTime OraAttuale
        {
            get
            {
                var fusoItaliano = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, fusoItaliano);
            }
        }
        public void Login(Utente utente)
        {
            UtenteLoggato = utente;
            NotifyStateChanged();
        }
        public void Logout()
        {
            UtenteLoggato = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}