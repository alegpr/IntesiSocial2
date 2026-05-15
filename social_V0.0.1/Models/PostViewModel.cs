namespace social_V0._0._1.Models
{
    public class PostViewModel
    {
        // Identificativi del post
        public int PostId { get; set; }
        public string Contenuto { get; set; } = string.Empty;
        public DateTime DataPubblicazione { get; set; }

        // Dati dell'autore
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;
        public string Dipartimento { get; set; } = string.Empty;
        public byte[]? FotoUrl { get; set; }

        // Logica dei Like (Aggiunta ora)
        public int LikeCount { get; set; }
        public bool IsLikedByMe { get; set; }

        // Proprietà calcolate per la UI
        public string? FotoUrlBase64 => FotoUrl != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(FotoUrl)}" : null;
        public string Iniziali => $"{(Nome?.Length > 0 ? Nome[0] : "")}{(Cognome?.Length > 0 ? Cognome[0] : "")}";
    }
}