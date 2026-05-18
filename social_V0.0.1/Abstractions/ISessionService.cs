using social_V0._0._1.Models;

namespace social_V0._0._1.Abstractions;

public interface ISessionService
{
    Utente? UtenteLoggato { get; }
    event Action? OnChange;
    DateTime OraAttuale { get; }
    void Login(Utente utente);
    void Logout();
}
