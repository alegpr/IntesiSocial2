using System;
using System.ComponentModel.DataAnnotations;

namespace social_V0._0._1.Models
{
    public class Utente
    {
        [Key]
        public int UtenteId { get; set; }
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;
        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [StringLength(50)]
        public string Cognome { get; set; } = string.Empty;
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido")]
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;
// Password hashata con BCrypt (MAI in chiaro).
        [Required(ErrorMessage = "La password è obbligatoria")]
        public string Password { get; set; } = string.Empty;
        [StringLength(50)]
        public string? Dipartimento { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DataDiNascita { get; set; }
        public DateTime DataCreazione { get; set; } = DateTime.Now;
        public byte[]? FotoUrl { get; set; }
    }
}