namespace social_V0._0._1.Models
{
    public class PostViewModel
    {
        public int PostId { get; set; }
        public string Contenuto { get; set; }
        public DateTime DataPubblicazione { get; set; }
        public int LikeCount { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Dipartimento { get; set; }
        public string FotoUrl { get; set; }
        public string Iniziali => $"{(Nome?.Length > 0 ? Nome[0] : "")}{(Cognome?.Length > 0 ? Cognome[0] : "")}";
    }
}
