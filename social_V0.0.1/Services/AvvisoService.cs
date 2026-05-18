namespace social_V0._0._1.Services
{
    //
    // Servizio Scoped per il recupero degli avvisi aziendali attivi.
    // Utilizza la vista dbo.VW_AvvisiAttivi che filtra per Attivo = 1,
    // centralizzando la logica di filtro a livello database.
    // Gli avvisi vengono visualizzati nella colonna laterale della home page.
    //
    public class AvvisoService : IAvvisoService
    {
        private readonly string _connectionString = string.Empty;

        //
        // Costruttore: riceve la configurazione tramite DI e recupera
        // la stringa di connessione "DefaultConnection" da appsettings.json.
        //
        public AvvisoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        //
        // Recupera tutti gli avvisi con Attivo = true, ordinati dal più recente
        // (DataAvviso DESC). La query utilizza la vista dbo.VW_AvvisiAttivi
        // per garantire consistenza e manutenibilità.
        //
        public async Task<List<Avviso>> GetAvvisiAttiviAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var avvisi = await connection.QueryAsync<Avviso>(
                    "SELECT * FROM dbo.VW_AvvisiAttivi ORDER BY DataAvviso DESC");
                return avvisi.ToList();
            }
        }
    }
}