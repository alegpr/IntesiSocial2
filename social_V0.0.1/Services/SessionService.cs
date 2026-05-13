using social_V0._0._1.Models;

namespace social_V0._0._1.Services
{
    public class SessionService
    {
        public Utente? UtenteLoggato { get; private set; }
        public bool IsAutenticato => UtenteLoggato != null;

        public void Login(Utente utente) => UtenteLoggato = utente;
        public void Logout() => UtenteLoggato = null;
    }
}