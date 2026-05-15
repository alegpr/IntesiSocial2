using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using social_V0._0._1.Models;

namespace social_V0._0._1.Services
{
    public class UtenteService
    {
        private readonly string _connectionString;

        public UtenteService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Utente> GetPrimoUtenteAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT TOP 1 UtenteId, Nome, Cognome, FotoUrl FROM dbo.Utenti ORDER BY UtenteId ASC";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Utente
                            {
                                UtenteId = (int)reader["UtenteId"],
                                Nome = reader["Nome"].ToString(),
                                Cognome = reader["Cognome"].ToString(),
                                FotoUrl = reader["FotoUrl"] as byte[]
                            };
                        }
                    }
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

        public async Task UpdateUtenteAsync(Utente utente)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                string sql;
                if (utente.FotoUrl != null)
                {
                    sql = "UPDATE dbo.Utenti SET Nome = @Nome, Cognome = @Cognome, Dipartimento = @Dipartimento, DataDiNascita = @DataDiNascita, FotoUrl = @FotoUrl WHERE UtenteId = @UtenteId";
                    await db.ExecuteAsync(sql, new { utente.Nome, utente.Cognome, utente.Dipartimento, utente.DataDiNascita, utente.FotoUrl, utente.UtenteId });
                }
                else
                {
                    sql = "UPDATE dbo.Utenti SET Nome = @Nome, Cognome = @Cognome, Dipartimento = @Dipartimento, DataDiNascita = @DataDiNascita WHERE UtenteId = @UtenteId";
                    await db.ExecuteAsync(sql, new { utente.Nome, utente.Cognome, utente.Dipartimento, utente.DataDiNascita, utente.UtenteId });
                }
            }
        }

        public async Task UpdatePasswordAsync(int utenteId, string nuovaPasswordHash)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync("UPDATE dbo.Utenti SET Password = @Password WHERE UtenteId = @UtenteId",
                    new { Password = nuovaPasswordHash, UtenteId = utenteId });
            }
        }

        public async Task<List<Utente>> GetCompleanniOggiAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"SELECT Nome, Cognome, FotoUrl FROM dbo.Utenti 
                    WHERE DAY(DataDiNascita) = DAY(GETDATE()) 
                    AND MONTH(DataDiNascita) = MONTH(GETDATE())";
                var result = await connection.QueryAsync<Utente>(sql);
                return result.ToList();
            }
        }

        public async Task<Utente?> LoginAsync(string email, string password)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "SELECT * FROM dbo.Utenti WHERE Email = @Email";
                var utente = await db.QueryFirstOrDefaultAsync<Utente>(sql, new { Email = email });

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
                var sql = @"INSERT INTO dbo.Utenti (Nome, Cognome, Email, Password, Dipartimento, DataDiNascita, DataCreazione, FotoUrl) 
                            VALUES (@Nome, @Cognome, @Email, @Password, @Dipartimento, @DataDiNascita, GETDATE(), @FotoUrl)";

                await db.ExecuteAsync(sql, new
                {
                    utente.Nome,
                    utente.Cognome,
                    utente.Email,
                    Password = passwordHash,
                    utente.Dipartimento,
                    utente.DataDiNascita,
                    FotoUrl = fotoUrl
                });
            }
        }
    }
}
