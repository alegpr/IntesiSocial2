using social_V0._0._1.Models;

namespace social_V0._0._1.Abstractions;

//
// Interfaccia del servizio di gestione degli avvisi aziendali.
// L'implementazione () utilizza
// la vista dbo.VW_AvvisiAttivi per ottenere solo gli avvisi con Attivo = true.
//
public interface IAvvisoService
{
    //
    // Recupera tutti gli avvisi attivi ordinati dal più recente.
    // Utilizzato nella colonna laterale della home page.
    //
    Task<List<Avviso>> GetAvvisiAttiviAsync();
}