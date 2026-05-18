namespace social_V0._0._1.Services
{
    public class UtenteService : IUtenteService
    {
        private readonly string _connectionString;
        public UtenteService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

// Usato per debug: recupera solo Nome/Cognome/FotoUrl del primo utente.
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
                            Nome = reader["Nome"].ToString(),
                            Cognome = reader["Cognome"].ToString(),
                            FotoUrl = reader["FotoUrl"] as byte[]
                        };
                }
            }
            return null;
        }
        public async Task<Utente?> GetUtenteByIdAsync(int utenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "SELECT * FROM dbo.Utenti WHERE UtenteId = @Id";
                return await db.QueryFirstOrDefaultAsync<Utente>(sql, new { Id = utenteId });
            }
        }

// Se FotoUrl è null, la foto nel DB non viene sovrascritta.
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

/* La nuova_password_hash deve già essere hashata con BCrypt dal chiamante
   (non viene hashata qui per non doppiare l'hashing). */
        public async Task UpdatePasswordAsync(int utenteId, string nuovaPasswordHash)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync("UPDATE dbo.Utenti SET Password = @Password WHERE UtenteId = @UtenteId",
                    new { Password = nuovaPasswordHash, UtenteId = utenteId });
            }
        }

// Confronta solo giorno e mese (ignora anno) con GETDATE() di SQL Server.
        public async Task<List<Utente>> GetCompleanniOggiAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"SELECT Nome, Cognome, FotoUrl FROM dbo.Utenti 
                    WHERE DAY(DataDiNascita) = DAY(GETDATE()) 
                    AND MONTH(DataDiNascita) = MONTH(GETDATE())";
                return (await connection.QueryAsync<Utente>(sql)).ToList();
            }
        }
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
