using social_V0._0._1.Models;

namespace social_V0._0._1.Abstractions;

public interface IPostService
{
    Task ToggleLikeAsync(int postId, int utenteId);
    Task InsertPostAsync(int utenteId, string contenuto);
    Task<List<PostViewModel>> GetAllPostsAsync(int mioUtenteId);
    Task<List<PostViewModel>> GetPostsByUtenteAsync(int utenteId);
}
