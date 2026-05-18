namespace social_V0._0._1.Models
{
    //
    // Modello di visualizzazione per un post nel feed.
    // Non mappa direttamente su una singola tabella, ma è il risultato
    // della vista SQL dbo.VW_PostFeed che combina dbo.Post e dbo.Utenti.
    // Include dati dell'autore (Nome, Cognome, Dipartimento, FotoUrl),
    // il conteggio dei like (LikeCount) e un flag
    // lato client che indica se l'utente loggato ha già messo like (IsLikedByMe).
    //
    public class PostViewModel
    {
        // Identificativo univoco del post.
        public int PostId { get; set; }

        // Testo del post inserito dall'utente.
        public string Contenuto { get; set; } = string.Empty;

        //
        // Data e ora di pubblicazione (UTC dal database).
        // Viene convertita nel fuso orario "W. Europe Standard Time"
        // al momento della visualizzazione nella UI.
        //
        public DateTime DataPubblicazione { get; set; }

        // Nome dell'autore del post.
        public string Nome { get; set; } = string.Empty;

        // Cognome dell'autore del post.
        public string Cognome { get; set; } = string.Empty;

        // Dipartimento dell'autore (mostrato come tag nel feed).
        public string Dipartimento { get; set; } = string.Empty;

        // Foto profilo dell'autore in formato byte[].
        public byte[]? FotoUrl { get; set; }

        //
        // Numero totale di like ricevuti dal post.
        // Calcolato dalla sottoquery COUNT nella vista VW_PostFeed.
        //
        public int LikeCount { get; set; }

        //
        // Flag calcolato lato server: indica se l'utente correntemente loggato
        // ha messo like a questo post. Viene popolato separatamente dal conteggio
        // tramite .
        // Usato nella UI per cambiare icona cuore (pieno/vuoto) e colore.
        //
        public bool IsLikedByMe { get; set; }

        //
        // Property calcolata: converte FotoUrl (byte[]) in una stringa base64
        // utilizzabile direttamente come src di un tag &lt;img&gt;.
        // Restituisce null se FotoUrl è null.
        //
        public string? FotoUrlBase64 =>
            FotoUrl != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(FotoUrl)}" : null;

        //
        // Property calcolata: restituisce le iniziali (prima lettera di Nome e Cognome).
        // Utilizzata come fallback quando l'utente non ha foto profilo.
        //
        public string Iniziali =>
            $"{(Nome?.Length > 0 ? Nome[0] : "")}{(Cognome?.Length > 0 ? Cognome[0] : "")}";
    }
}