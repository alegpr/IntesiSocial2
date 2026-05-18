using social_V0._0._1.Models;

namespace social_V0._0._1.Abstractions;

public interface IUtenteService
{
    Task<Utente?> GetPrimoUtenteAsync();
    Task<Utente?> GetUtenteByIdAsync(int utenteId);
    Task UpdateUtenteAsync(Utente utente);
    Task UpdatePasswordAsync(int utenteId, string nuovaPasswordHash);
    Task<List<Utente>> GetCompleanniOggiAsync();
    Task<Utente?> LoginAsync(string email, string password);
    Task RegisterAsync(Utente utente, byte[]? fotoUrl);
}
