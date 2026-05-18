using social_V0._0._1.Models;

namespace social_V0._0._1.Abstractions;

//
// Interfaccia del servizio di sessione utente.
// Definisce il contratto per la gestione dello stato dell'utente loggato
// all'interno del circuito SignalR di Blazor Server.
// L'implementazione concreta ()
// è registrata come Scoped: ogni circuito SignalR ha la propria istanza.
//
public interface ISessionService
{
    // Utente correntemente loggato. Null se nessuno è autenticato.
    Utente? UtenteLoggato { get; }

    //
    // Evento notificato quando lo stato di login/logout cambia.
    // I componenti UI (es. UserMenu) si sottoscrivono per aggiornarsi automaticamente.
    //
    event Action? OnChange;

    //
    // Ora corrente nel fuso orario italiano (W. Europe Standard Time).
    // Property calcolata, non memorizzata nello stato della sessione.
    //
    DateTime OraAttuale { get; }

    // Imposta l'utente come loggato e notifica i listener di OnChange.
    void Login(Utente utente);

    // Resetta lo stato a non loggato e notifica i listener di OnChange.
    void Logout();
}