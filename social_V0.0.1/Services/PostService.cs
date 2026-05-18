using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.Data.SqlClient; // Assicurati di avere questo
using Dapper;                 // E questo
using social_V0._0._1.Models;  // Cambia in base al tuo namespace reale

namespace social_V0._0._1.Services
{
    public class PostService : IPostService
    {
        private readonly string _connectionString;
        private readonly IDistributedCache _cache;

        // Chiavi per Redis
        private const string GlobalFeedKey = "feed_globale_posts";
        private string UserLikesKey(int userId) => $"user_likes_{userId}";

        public PostService(IConfiguration configuration, IDistributedCache cache)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _cache = cache;
        }

        public async Task<List<PostViewModel>> GetAllPostsAsync(int mioUtenteId)
        {
            // 1. Tenta di recuperare i post dalla cache globale
            List<PostViewModel> posts;
            var cachedPosts = await _cache.GetStringAsync(GlobalFeedKey);

            if (!string.IsNullOrEmpty(cachedPosts))
            {
                posts = JsonSerializer.Deserialize<List<PostViewModel>>(cachedPosts);
            }
            else
            {
                // 2. Cache Miss: Query al DB (senza IsLikedByMe perché è globale)
                using (var db = new SqlConnection(_connectionString))
                {
                    string sql = @"
                        SELECT P.PostId, P.Contenuto, P.DataPubblicazione,
                               U.Nome, U.Cognome, U.Dipartimento, U.FotoUrl,
                               (SELECT COUNT(*) FROM dbo.PostLikes WHERE PostId = P.PostId) AS LikeCount
                        FROM dbo.Post P
                        INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId
                        ORDER BY P.DataPubblicazione DESC";

                    posts = (await db.QueryAsync<PostViewModel>(sql)).ToList();

                    // 3. Salva in Memurai per 5 secondi (abbastanza per il tuo loop da 500ms)
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                    };
                    await _cache.SetStringAsync(GlobalFeedKey, JsonSerializer.Serialize(posts), cacheOptions);
                }
            }

            // 4. Gestione dinamica dei Like per l'utente loggato
            // Recuperiamo quali post hanno il like dell'utente da una cache specifica o DB
            var likedPostIds = await GetUserLikedElementIdsAsync(mioUtenteId);

            foreach (var post in posts)
            {
                post.IsLikedByMe = likedPostIds.Contains(post.PostId);
            }

            return posts;
        }

        private async Task<HashSet<int>> GetUserLikedElementIdsAsync(int utenteId)
        {
            string key = UserLikesKey(utenteId);
            var cached = await _cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<HashSet<int>>(cached);
            }

            using (var db = new SqlConnection(_connectionString))
            {
                var ids = (await db.QueryAsync<int>(
                    "SELECT PostId FROM dbo.PostLikes WHERE UtenteId = @UtenteId",
                    new { UtenteId = utenteId })).ToHashSet();

                await _cache.SetStringAsync(key, JsonSerializer.Serialize(ids),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) });

                return ids;
            }
        }

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

            // Invalida le cache interessate
            await _cache.RemoveAsync(GlobalFeedKey);
            await _cache.RemoveAsync(UserLikesKey(utenteId));
        }

        public async Task InsertPostAsync(int utenteId, string contenuto)
        {
            if (string.IsNullOrWhiteSpace(contenuto)) return;

            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "INSERT INTO dbo.Post (UtenteId, Contenuto, DataPubblicazione) VALUES (@uId, @cont, GETDATE())";
                await connection.ExecuteAsync(sql, new { uId = utenteId, cont = contenuto });
            }

            // Nuovo post? Tabula rasa della cache dei post
            await _cache.RemoveAsync(GlobalFeedKey);
        }

        public async Task<List<PostViewModel>> GetPostsByUtenteAsync(int utenteId)
        {
            // Per i profili singoli possiamo saltare la cache o usarne una specifica se necessario
            using (var db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT P.PostId, P.Contenuto, P.DataPubblicazione,
                           U.Nome, U.Cognome, U.Dipartimento, U.FotoUrl,
                           (SELECT COUNT(*) FROM dbo.PostLikes WHERE PostId = P.PostId) AS LikeCount,
                           CAST(CASE WHEN EXISTS (
                               SELECT 1 FROM dbo.PostLikes WHERE PostId = P.PostId AND UtenteId = @UtenteId
                           ) THEN 1 ELSE 0 END AS BIT) AS IsLikedByMe
                    FROM dbo.Post P
                    INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId
                    WHERE P.UtenteId = @UtenteId
                    ORDER BY P.DataPubblicazione DESC";

                return (await db.QueryAsync<PostViewModel>(sql, new { UtenteId = utenteId })).ToList();
            }
        }
    }
}