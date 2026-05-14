namespace social_V0._0._1.Models
{
    public class PostViewModel
    {
        public int PostId { get; set; }
        public string Contenuto { get; set; } = string.Empty;
        public DateTime DataPubblicazione { get; set; }
        public int LikeCount { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;
        public string Dipartimento { get; set; } = string.Empty;
        public byte[]? FotoUrl { get; set; }
        public string? FotoUrlBase64 => FotoUrl != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(FotoUrl)}" : null;
        public string Iniziali => $"{(Nome?.Length > 0 ? Nome[0] : "")}{(Cognome?.Length > 0 ? Cognome[0] : "")}";
    }
}
