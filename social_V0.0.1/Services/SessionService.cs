using social_V0._0._1.Models;

namespace social_V0._0._1.Services
{
    public class SessionService
    {
        // Questa è la proprietà che la Home va a leggere
        public Utente? UtenteLoggato { get; private set; }

        public void Login(Utente utente)
        {
            UtenteLoggato = utente;
        }

        public void Logout()
        {
            UtenteLoggato = null;
        }
    }
}