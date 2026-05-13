using Dapper;
using Microsoft.Data.SqlClient;
using social_V0._0._1.Models;

namespace social_V0._0._1.Services
{
    public class PostService
    {
        private readonly string _connectionString = "Server=192.168.16.215,1433;Database=db.social;User Id=app_user;Password=2026Intesi;TrustServerCertificate=True;";

        public async Task<Utente> GetPrimoUtenteAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // Cerchiamo Pierpaolo o il primo utente disponibile
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
                                FotoUrl = reader["FotoUrl"]?.ToString()
                            };
                        }
                    }
                }
            }
            return null; // Se restituisce null, il database è vuoto!
        }
        public async Task InsertPostAsync(int utenteId, string contenuto)
        {
            if (string.IsNullOrWhiteSpace(contenuto)) return;

            using (var connection = new SqlConnection(_connectionString))
            {
                // Uniformato a dbo.Post (singolare) come in GetAllPostsAsync
                var sql = "INSERT INTO dbo.Post (UtenteId, Contenuto, DataPubblicazione) VALUES (@uId, @cont, GETDATE())";
                await connection.ExecuteAsync(sql, new { uId = utenteId, cont = contenuto });
            }
        }
        public async Task<List<PostViewModel>> GetAllPostsAsync()
        {
            var lista = new List<PostViewModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT P.PostId, P.Contenuto, P.DataPubblicazione, 
                                      U.Nome, U.Cognome, U.Dipartimento, U.FotoUrl
                               FROM dbo.Post P
                               INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId
                               ORDER BY P.DataPubblicazione DESC";

                var command = new SqlCommand(sql, connection);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new PostViewModel
                        {
                            PostId = (int)reader["PostId"],
                            Contenuto = reader["Contenuto"].ToString(),
                            DataPubblicazione = (DateTime)reader["DataPubblicazione"],
                            Nome = reader["Nome"]?.ToString() ?? "",
                            Cognome = reader["Cognome"]?.ToString() ?? "",
                            Dipartimento = reader["Dipartimento"]?.ToString() ?? "N/D",
                            FotoUrl = reader["FotoUrl"] == DBNull.Value ? null : reader["FotoUrl"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
        public async Task<List<Avviso>> GetAvvisiAttiviAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Recuperiamo solo gli avvisi segnati come "Attivo = 1"
                var sql = "SELECT * FROM dbo.Avvisi WHERE Attivo = 1 ORDER BY DataAvviso DESC";
                var avvisi = await connection.QueryAsync<Avviso>(sql);
                return avvisi.ToList();
            }
        }
        public async Task<List<Utente>> GetCompleanniOggiAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Nota: uso DataDiNascita come vedo nel tuo screenshot
                var sql = @"SELECT Nome, Cognome FROM dbo.Utenti 
                    WHERE DAY(DataDiNascita) = DAY(GETDATE()) 
                    AND MONTH(DataDiNascita) = MONTH(GETDATE())";
                var result = await connection.QueryAsync<Utente>(sql);
                return result.ToList();
            }
        }
    }
}