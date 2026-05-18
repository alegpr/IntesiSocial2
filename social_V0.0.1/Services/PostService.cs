using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using social_V0._0._1.Models;

namespace social_V0._0._1.Services
{
    public class PostService
    {
        private readonly string _connectionString;
        public PostService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

// Toggle: se il like esiste lo elimina, altrimenti lo inserisce.
        public async Task ToggleLikeAsync(int postId, int utenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var esiste = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM dbo.PostLikes WHERE PostId = @PostId AND UtenteId = @UtenteId",
                    new { PostId = postId, UtenteId = utenteId });

                if (esiste > 0)
                    await db.ExecuteAsync("DELETE FROM dbo.PostLikes WHERE PostId = @PostId AND UtenteId = @UtenteId",
                        new { PostId = postId, UtenteId = utenteId });
                else
                    await db.ExecuteAsync("INSERT INTO dbo.PostLikes (PostId, UtenteId) VALUES (@PostId, @UtenteId)",
                        new { PostId = postId, UtenteId = utenteId });
            }
        }
        public async Task InsertPostAsync(int utenteId, string contenuto)
        {
            if (string.IsNullOrWhiteSpace(contenuto)) return;

            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "INSERT INTO dbo.Post (UtenteId, Contenuto, DataPubblicazione) VALUES (@uId, @cont, GETDATE())";
                await connection.ExecuteAsync(sql, new { uId = utenteId, cont = contenuto });
            }
        }
// Include conteggio like e flag "IsLikedByMe" per l'utente loggato.

        public async Task<List<PostViewModel>> GetAllPostsAsync(int mioUtenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT P.PostId, P.Contenuto, P.DataPubblicazione,
                           U.Nome, U.Cognome, U.Dipartimento, U.FotoUrl,
                           (SELECT COUNT(*) FROM dbo.PostLikes WHERE PostId = P.PostId) AS LikeCount,
                           CAST(CASE WHEN EXISTS (
                               SELECT 1 FROM dbo.PostLikes WHERE PostId = P.PostId AND UtenteId = @MioId
                           ) THEN 1 ELSE 0 END AS BIT) AS IsLikedByMe
                    FROM dbo.Post P
                    INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId
                    ORDER BY P.DataPubblicazione DESC";

                return (await db.QueryAsync<PostViewModel>(sql, new { MioId = mioUtenteId })).ToList();
            }
        }
// Stessa struttura di GetAllPostsAsync, filtrata per un singolo autore.

        public async Task<List<PostViewModel>> GetPostsByUtenteAsync(int utenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT P.PostId, P.Contenuto, P.DataPubblicazione,
                           U.Nome, U.Cognome, U.Dipartimento, U.FotoUrl,
                           (SELECT COUNT(*) FROM dbo.PostLikes WHERE PostId = P.PostId) AS LikeCount,
                           CAST(CASE WHEN EXISTS (
                               SELECT 1 FROM dbo.PostLikes WHERE PostId = P.PostId AND UtenteId = @MioId
                           ) THEN 1 ELSE 0 END AS BIT) AS IsLikedByMe
                    FROM dbo.Post P
                    INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId
                    WHERE P.UtenteId = @UtenteId
                    ORDER BY P.DataPubblicazione DESC";

                return (await db.QueryAsync<PostViewModel>(sql, new { UtenteId = utenteId, MioId = utenteId })).ToList();
            }
        }
    }
}
