using social_V0._0._1.Models;

namespace social_V0._0._1.Abstractions;

//
// Interfaccia del servizio di gestione dei post e dei like.
// L'implementazione () utilizza
// Dapper per le operazioni SQL e Memurai (Redis compatibile) come
// cache distribuita per ridurre il carico sul database.
//
public interface IPostService
{
    //
    // Toggle like: se l'utente ha già messo like, lo rimuove; altrimenti lo aggiunge.
    // Invalida le cache dei post e dei like dell'utente dopo l'operazione.
    //
    Task ToggleLikeAsync(int postId, int utenteId);

    //
    // Inserisce un nuovo post nel database.
    // Invalida la cache globale dei post per forzare il refresh del feed.
    //
    Task InsertPostAsync(int utenteId, string contenuto);

    //
    // Recupera tutti i post per il feed globale (home page).
    // Utilizza la cache distribuita per evitare query DB ripetute.
    // Per ogni post calcola se l'utente loggato ha messo like.
    //
    Task<List<PostViewModel>> GetAllPostsAsync(int mioUtenteId);

    //
    // Recupera i post di un singolo utente per la pagina profilo.
    // Anche qui calcola IsLikedByMe lato server per coerenza con il feed.
    //
    Task<List<PostViewModel>> GetPostsByUtenteAsync(int utenteId);
}