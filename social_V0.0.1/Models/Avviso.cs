namespace social_V0._0._1.Models
{
    //
    // Modello che rappresenta un avviso aziendale.
    // Mappa sulla tabella dbo.Avvisi del database SQL Server.
    // Gli avvisi vengono mostrati nella colonna laterale della home page
    // e sono filtrati esclusivamente per quelli con Attivo = true
    // tramite la vista dbo.VW_AvvisiAttivi.
    //
    public class Avviso
    {
        // Identificativo univoco dell'avviso.
        public int AvvisoId { get; set; }

        // Titolo sintetico dell'avviso (es. "Sciopero mezzi 18/05").
        public string Titolo { get; set; } = string.Empty;

        // Corpo del messaggio con i dettagli dell'avviso.
        public string Messaggio { get; set; } = string.Empty;

        //
        // Categoria/tipo dell'avviso (es. "Generale", "URGENTE", "Risorse Umane").
        // Può essere usata in futuro per filtraggio o colorazione diversa nella UI.
        //
        public string Tipo { get; set; } = string.Empty;

        // Data di pubblicazione dell'avviso. Ordinamento discendente nella UI.
        public DateTime DataAvviso { get; set; }

        //
        // Flag soft-delete: se false l'avviso viene nascosto dalla vista VW_AvvisiAttivi
        // ma rimane persistito nel database per audit.
        //
        public bool Attivo { get; set; }
    }
}