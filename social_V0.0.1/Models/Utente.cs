using System;
using System.ComponentModel.DataAnnotations;

namespace social_V0._0._1.Models
{
    /// <summary>
    /// Rappresenta l'entità Utente allineata alla tabella dbo.Utenti del database.
    /// </summary>
    public class Utente
    {
        // --- IDENTIFICATIVI ---
        [Key]
        public int UtenteId { get; set; }

        // --- INFORMAZIONI ANAGRAFICHE ---
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [StringLength(50)]
        public string Cognome { get; set; } = string.Empty;

        // --- CREDENZIALI E CONTATTI ---
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido")]
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La password è obbligatoria")]
        public string Password { get; set; } = string.Empty;

        // --- DETTAGLI AZIENDALI E PERSONALI ---
        [StringLength(50)]
        public string? Dipartimento { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataDiNascita { get; set; }

        // --- METADATI DI SISTEMA ---
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        public byte[]? FotoUrl { get; set; }
    }
}