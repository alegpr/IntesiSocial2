using System;
using System.ComponentModel.DataAnnotations;

namespace social_V0._0._1.Models
{
    //
    // Modello che rappresenta un utente dell'applicazione.
    // Mappa direttamente sulla tabella dbo.Utenti del database SQL Server.
    // Le property con attributi di validazione (Required, StringLength, EmailAddress)
    // vengono utilizzate sia da Blazor (RadzenTemplateForm) lato client,
    // sia dal server per la validazione del modello prima del salvataggio.
    // La proprietà Password contiene l'hash BCrypt, mai la password in chiaro.
    //
    public class Utente
    {
        // Identificativo univoco auto-generato dal database (IDENTITY).
        [Key]
        public int UtenteId { get; set; }

        // Nome dell'utente. Campo obbligatorio, max 50 caratteri.
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        // Cognome dell'utente. Campo obbligatorio, max 50 caratteri.
        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [StringLength(50)]
        public string Cognome { get; set; } = string.Empty;

        //
        // Email aziendale utilizzata come identificativo di login.
        // Deve essere univoca e in formato valido.
        //
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido")]
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;

        //
        // Password hashata con BCrypt (MAI in chiaro).
        // L'hashing viene eseguito esclusivamente in
        // e verificato in .
        //
        [Required(ErrorMessage = "La password è obbligatoria")]
        public string Password { get; set; } = string.Empty;

        // Dipartimento di appartenenza dell'utente (es. "IT", "HR", "Marketing"). Opzionale.
        [StringLength(50)]
        public string? Dipartimento { get; set; }

        // Data di nascita, utilizzata per la sezione "Compleanni" nella home page.
        [DataType(DataType.Date)]
        public DateTime? DataDiNascita { get; set; }

        //
        // Data di creazione/registrazione dell'account.
        // Impostata automaticamente in C# con default DateTime.Now,
        // ma viene sovrascritta da GETDATE() nella INSERT SQL.
        //
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        //
        // Foto profilo dell'utente salvata come byte[] (varbinary su SQL Server).
        // Può essere null. Se presente, viene convertita in base64 tramite
        // per la visualizzazione nei tag &lt;img&gt;.
        //
        public byte[]? FotoUrl { get; set; }
        public bool IsAdmin { get; set; }
    }
}