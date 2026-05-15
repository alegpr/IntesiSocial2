namespace social_V0._0._1.Models
{
    public class Avviso
    {
        public int AvvisoId { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string Messaggio { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime DataAvviso { get; set; }
        public bool Attivo { get; set; }
    }
}
