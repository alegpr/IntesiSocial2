namespace social_V0._0._1.Services
{
    //
    // Servizio Scoped per la gestione degli utenti: login, registrazione,
    // aggiornamento profilo, cambio password e compleanni.
    // Utilizza Dapper per query SQL dirette sulla tabella dbo.Utenti
    // e BCrypt per l'hashing/verifica delle password.
    // La connection string è letta da appsettings.json tramite IConfiguration.
    //
    public class UtenteService : IUtenteService
    {
        private readonly string _connectionString = string.Empty;

        //
        // Costruttore: riceve la configurazione tramite DI e recupera
        // la stringa di connessione "DefaultConnection".
        //
        public UtenteService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        //
        // Recupera il primo utente del database ordinato per UtenteId.
        // Usato solo per debug/demo iniziale del profilo.
        //
        public async Task<Utente?> GetPrimoUtenteAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT TOP 1 UtenteId, Nome, Cognome, FotoUrl FROM dbo.Utenti ORDER BY UtenteId ASC";
                using (var command = new SqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return new Utente
                        {
                            UtenteId = (int)reader["UtenteId"],
                            Nome = reader["Nome"]?.ToString() ?? string.Empty,
                            Cognome = reader["Cognome"]?.ToString() ?? string.Empty,
                            FotoUrl = reader["FotoUrl"] as byte[]
                        };
                }
            }
            return null;
        }

        //
        // Recupera un utente completo per ID.
        // Utilizzato da MainLayout per ripristinare la sessione dal cookie userId
        // dopo un refresh di pagina o navigazione con forceLoad.
        //
        public async Task<Utente?> GetUtenteByIdAsync(int utenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "SELECT * FROM dbo.Utenti WHERE UtenteId = @Id";
                return await db.QueryFirstOrDefaultAsync<Utente>(sql, new { Id = utenteId });
            }
        }

        //
        // Aggiorna i dati anagrafici dell'utente.
        // Se FotoUrl è null, la foto nel DB non viene sovrascritta
        // (evita di cancellare accidentalmente la foto esistente).
        //
        public async Task UpdateUtenteAsync(Utente utente)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                if (utente.FotoUrl != null)
                {
                    await db.ExecuteAsync(
                        "UPDATE dbo.Utenti SET Nome=@Nome, Cognome=@Cognome, Dipartimento=@Dipartimento, DataDiNascita=@DataDiNascita, FotoUrl=@FotoUrl WHERE UtenteId=@UtenteId",
                        new { utente.Nome, utente.Cognome, utente.Dipartimento, utente.DataDiNascita, utente.FotoUrl, utente.UtenteId });
                }
                else
                {
                    await db.ExecuteAsync(
                        "UPDATE dbo.Utenti SET Nome=@Nome, Cognome=@Cognome, Dipartimento=@Dipartimento, DataDiNascita=@DataDiNascita WHERE UtenteId=@UtenteId",
                        new { utente.Nome, utente.Cognome, utente.Dipartimento, utente.DataDiNascita, utente.UtenteId });
                }
            }
        }

        //
        // Aggiorna la password dell'utente.
        // deve già essere hashata con BCrypt
        // dal chiamante (Profilo.razor) per evitare doppio hashing.
        //
        public async Task UpdatePasswordAsync(int utenteId, string nuovaPasswordHash)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync("UPDATE dbo.Utenti SET Password = @Password WHERE UtenteId = @UtenteId",
                    new { Password = nuovaPasswordHash, UtenteId = utenteId });
            }
        }

        //
        // Recupera l'elenco degli utenti che compiono gli anni oggi,
        // confrontando giorno e mese (ignorando l'anno) con GETDATE().
        // La query usa la vista dbo.VW_CompleanniOggi.
        //
        public async Task<List<Utente>> GetCompleanniOggiAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return (await connection.QueryAsync<Utente>(
                    "SELECT * FROM dbo.VW_CompleanniOggi")).ToList();
            }
        }

        //
        // Tenta l'autenticazione: cerca l'utente per Email, poi verifica
        // la password con BCrypt.Verify(). Se corrisponde, restituisce l'utente;
        // altrimenti restituisce null (credenziali errate).
        //
        public async Task<Utente?> LoginAsync(string email, string password)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var utente = await db.QueryFirstOrDefaultAsync<Utente>(
                    "SELECT * FROM dbo.Utenti WHERE Email = @Email", new { Email = email });

                if (utente != null && global::BCrypt.Net.BCrypt.Verify(password, utente.Password))
                    return utente;

                return null;
            }
        }

        //
        // Registra un nuovo utente. La password viene prima hashata con BCrypt,
        // poi salvata nel database. DataCreazione è impostata a GETDATE() dal server SQL.
        //
        public async Task RegisterAsync(Utente utente, byte[]? fotoUrl)
        {
            string passwordHash = global::BCrypt.Net.BCrypt.HashPassword(utente.Password);

            using (var db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(@"
                    INSERT INTO dbo.Utenti (Nome, Cognome, Email, Password, Dipartimento, DataDiNascita, DataCreazione, FotoUrl)
                    VALUES (@Nome, @Cognome, @Email, @Password, @Dipartimento, @DataDiNascita, GETDATE(), @FotoUrl)",
                    new
                    {
                        utente.Nome, utente.Cognome, utente.Email, Password = passwordHash,
                        utente.Dipartimento, utente.DataDiNascita, FotoUrl = fotoUrl
                    });
            }
        }
    }
}