using Dapper;
using Microsoft.Data.SqlClient;
using social_V0._0._1.Models;

namespace social_V0._0._1.Services
{
    public class PostService
    {
        private readonly string _connectionString = "Server=tcp:sql-fsl-intesi-2026.database.windows.net,1433;Database=fsl-sql-intesi-2026;User Id=fsl-admin;Password=Password1;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

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
                                FotoUrl = reader["FotoUrl"] as byte[]
                            };
                        }
                    }
                }
            }
            return null; // Se restituisce null, il database è vuoto!
        }
        public async Task ToggleLikeAsync(int postId, int utenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                // Controlliamo se il like esiste già
                var esiste = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM dbo.PostLikes WHERE PostId = @PostId AND UtenteId = @UtenteId",
                    new { PostId = postId, UtenteId = utenteId });

                if (esiste > 0)
                {
                    // Se esiste, lo togliamo (Unlike)
                    await db.ExecuteAsync(
                        "DELETE FROM dbo.PostLikes WHERE PostId = @PostId AND UtenteId = @UtenteId",
                        new { PostId = postId, UtenteId = utenteId });
                }
                else
                {
                    // Se non esiste, lo aggiungiamo (Like)
                    await db.ExecuteAsync(
                        "INSERT INTO dbo.PostLikes (PostId, UtenteId) VALUES (@PostId, @UtenteId)",
                        new { PostId = postId, UtenteId = utenteId });
                }
            }
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
        public async Task<List<PostViewModel>> GetAllPostsAsync(int mioUtenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                // Query ottimizzata: recupera post, autore, conteggio like e stato del mio like
                string sql = @"
            SELECT 
                P.PostId, P.Contenuto, P.DataPubblicazione, 
                U.Nome, U.Cognome, U.Dipartimento, U.FotoUrl,
                (SELECT COUNT(*) FROM dbo.PostLikes WHERE PostId = P.PostId) AS LikeCount,
                CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM dbo.PostLikes 
                    WHERE PostId = P.PostId AND UtenteId = @MioId
                ) THEN 1 ELSE 0 END AS BIT) AS IsLikedByMe
            FROM dbo.Post P
            INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId
            ORDER BY P.DataPubblicazione DESC";

                // Usiamo Dapper per mappare tutto automaticamente al ViewModel
                var result = await db.QueryAsync<PostViewModel>(sql, new { MioId = mioUtenteId });
                return result.ToList();
            }
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