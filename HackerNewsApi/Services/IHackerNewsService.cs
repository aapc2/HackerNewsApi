using HackerNewsApi.Models;

namespace HackerNewsApi.Services
{
    public interface IHackerNewsService
    {
        Task<List<StoryResponse>> GetBestStoriesAsync(int n, CancellationToken cancellationToken = default);
    }
}