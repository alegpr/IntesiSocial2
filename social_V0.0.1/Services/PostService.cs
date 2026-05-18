using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace social_V0._0._1.Services
{
    //
    // Servizio Scoped per la gestione dei post e dei like.
    // Utilizza un pattern Cache-Aside con Memurai (Redis compatibile)
    // per ridurre le query sul database nel feed globale (polling ogni 500ms).
    // Le viste SQL dbo.VW_PostFeed e dbo.VW_UtenteLikes centralizzano
    // la logica di join tra le tabelle Post, Utenti e PostLikes.
    //
    public class PostService : IPostService
    {
        private readonly string _connectionString = string.Empty;
        private readonly IDistributedCache _cache;

        // Chiave Redis per la cache globale del feed post (TTL 5s).
        private const string GlobalFeedKey = "feed_globale_posts";

        // Chiave Redis per i like di un utente (%userId%). Templated, TTL 10s.
        private string UserLikesKey(int userId) => $"user_likes_{userId}";

        //
        // Costruttore: riceve configuration (per la connection string) e
        // IDistributedCache (Memurai/Redis). Il cache è registrato in Program.cs
        // con AddStackExchangeRedisCache().
        //
        public PostService(IConfiguration configuration, IDistributedCache cache)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _cache = cache;
        }

        //
        // Recupera tutti i post per il feed globale della home page.
        // Strategy Cache-Aside:
        // 1. Tenta il read from Memurai (chiave "feed_globale_posts").
        // 2. Se cache hit → deserializza e procede.
        // 3. Se cache miss → query DB tramite VW_PostFeed, salva in cache (TTL 5s).
        // 4. Recupera i like dell'utente dalla cache utente-specifica o DB.
        // 5. Imposta IsLikedByMe su ogni post in base ai like recuperati.
        // Il TTL di 5s garantisce che il polling ogni 500ms non saturi il DB.
        //
        public async Task<List<PostViewModel>> GetAllPostsAsync(int mioUtenteId)
        {
            List<PostViewModel> posts;
            var cachedPosts = await _cache.GetStringAsync(GlobalFeedKey);

            if (!string.IsNullOrEmpty(cachedPosts))
            {
                posts = JsonSerializer.Deserialize<List<PostViewModel>>(cachedPosts) ?? new List<PostViewModel>();
            }
            else
            {
                using (var db = new SqlConnection(_connectionString))
                {
                    posts = (await db.QueryAsync<PostViewModel>(
                        "SELECT * FROM dbo.VW_PostFeed ORDER BY DataPubblicazione DESC")).ToList();

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                    };
                    await _cache.SetStringAsync(GlobalFeedKey, JsonSerializer.Serialize(posts), cacheOptions);
                }
            }

            var likedPostIds = await GetUserLikedElementIdsAsync(mioUtenteId);

            foreach (var post in posts)
            {
                post.IsLikedByMe = likedPostIds.Contains(post.PostId);
            }

            return posts;
        }

        //
        // Recupera l'insieme degli ID dei post a cui l'utente ha messo like.
        // Anche questo metodo usa la cache (chiave "user_likes_{userId}", TTL 10s).
        // La cache viene invalidata da  dopo ogni operazione.
        //
        private async Task<HashSet<int>> GetUserLikedElementIdsAsync(int utenteId)
        {
            string key = UserLikesKey(utenteId);
            var cached = await _cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<HashSet<int>>(cached) ?? new HashSet<int>();
            }

            using (var db = new SqlConnection(_connectionString))
            {
                var ids = (await db.QueryAsync<int>(
                    "SELECT PostId FROM dbo.VW_UtenteLikes WHERE UtenteId = @UtenteId",
                    new { UtenteId = utenteId })).ToHashSet();

                await _cache.SetStringAsync(key, JsonSerializer.Serialize(ids),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) });

                return ids;
            }
        }

        //
        // Toggle del like: se esiste lo elimina (unlike), altrimenti lo inserisce (like).
        // Entrambe le cache (feed globale e like utente) vengono invalidate dopo l'operazione
        // per garantire consistenza al prossimo refresh.
        //
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

            await _cache.RemoveAsync(GlobalFeedKey);
            await _cache.RemoveAsync(UserLikesKey(utenteId));
        }

        //
        // Inserisce un nuovo post nel database con la data corrente (GETDATE()).
        // Invalida la cache globale dei post per forzare il refresh del feed.
        //
        public async Task InsertPostAsync(int utenteId, string contenuto)
        {
            if (string.IsNullOrWhiteSpace(contenuto)) return;

            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "INSERT INTO dbo.Post (UtenteId, Contenuto, DataPubblicazione) VALUES (@uId, @cont, GETDATE())";
                await connection.ExecuteAsync(sql, new { uId = utenteId, cont = contenuto });
            }

            await _cache.RemoveAsync(GlobalFeedKey);
        }

        //
        // Recupera i post di un singolo utente (per la pagina profilo).
        // Filtro via WHERE UtenteId sulla vista VW_PostFeed.
        // I like sono calcolati allo stesso modo di GetAllPostsAsync per coerenza.
        //
        public async Task<List<PostViewModel>> GetPostsByUtenteAsync(int utenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var posts = (await db.QueryAsync<PostViewModel>(
                    "SELECT * FROM dbo.VW_PostFeed WHERE UtenteId = @UtenteId ORDER BY DataPubblicazione DESC",
                    new { UtenteId = utenteId })).ToList();

                var likedPostIds = await GetUserLikedElementIdsAsync(utenteId);
                foreach (var post in posts)
                    post.IsLikedByMe = likedPostIds.Contains(post.PostId);

                return posts;
            }
        }
    }
}