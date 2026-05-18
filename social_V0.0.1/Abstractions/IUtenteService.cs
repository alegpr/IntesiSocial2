using social_V0._0._1.Models;

namespace social_V0._0._1.Abstractions
{
    //
    // Interfaccia del servizio di gestione utenti.
    // Contiene le operazioni CRUD e di autenticazione sul database.
    // L'implementazione (UtenteService) utilizza
    // Dapper per query SQL dirette e BCrypt per l'hashing delle password.
    //
    public interface IUtenteService
    {
        // Recupera il primo utente del database (usato per debug/demo).
        Task<Utente?> GetPrimoUtenteAsync();

        // Recupera un utente per ID (usato nel ripristino sessione da cookie).
        Task<Utente?> GetUtenteByIdAsync(int utenteId);

        // Aggiorna i dati anagrafici di un utente (nome, cognome, dipartimento, data nascita, foto).
        Task UpdateUtenteAsync(Utente utente);

        // Aggiorna la password hashata di un utente (la nuova_password_hash deve già essere hashata con BCrypt).
        Task UpdatePasswordAsync(int utenteId, string nuovaPasswordHash);

        // Elenco degli utenti che compiono gli anni oggi, per la sezione Compleanni della home.
        Task<List<Utente>> GetCompleanniOggiAsync();

        //
        // Tenta l'autenticazione: cerca utente per email e verifica password con BCrypt.
        // Restituisce l'utente se credenziali valide, null altrimenti.
        //
        Task<Utente?> LoginAsync(string email, string password);

        // Registra un nuovo utente nel database con password hashata.
        Task RegisterAsync(Utente utente, byte[]? fotoUrl);

        //
        // Recupera le statistiche dei dipendenti raggruppati per dipartimento.
        // Usato nella Dashboard Amministratore per visualizzare la distribuzione del personale.
        //
        Task<Dictionary<string, int>> GetUtentiPerDipartimentoAsync();
    }
}