using HackerNewsApi.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace HackerNewsApi.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private const string RankedStoriesCacheKey = "ranked-stories";
        private const int MaxConcurrency = 20;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        public HackerNewsService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task<List<StoryResponse>> GetBestStoriesAsync(int n, CancellationToken cancellationToken = default)
        {
            // Return cached stories to avoid unnecessary calls to the Hacker News API
            if (_cache.TryGetValue(RankedStoriesCacheKey, out List<StoryResponse>? cachedStories) && cachedStories != null)
            {
                return cachedStories.Take(n).ToList();
            }

            // Ensure only one request refreshes the cache at a time
            await _refreshLock.WaitAsync(cancellationToken);

            try
            {
                // Another request may have populated the cache while this one was waiting
                if (_cache.TryGetValue(RankedStoriesCacheKey, out cachedStories) && cachedStories != null)
                {
                    return cachedStories.Take(n).ToList();
                }

                var client = _httpClientFactory.CreateClient("HackerNews");

                // Fetch story IDs asynchronously without blocking the request thread
                var storyIds = await client.GetFromJsonAsync<List<int>>("v0/beststories.json",cancellationToken);

                // Return an empty result if Hacker News provides no story IDs
                if (storyIds == null || storyIds.Count == 0)
                {
                    return new List<StoryResponse>();
                }

                // Limit concurrent requests to avoid overloading the Hacker News API
                using var semaphore = new SemaphoreSlim(MaxConcurrency);

                // Execute multiple story requests concurrently to reduce total response time
                var tasks = storyIds.Select(async id =>
                {
                    await semaphore.WaitAsync(cancellationToken);

                    try
                    {
                        return await GetStoryAsync(client, id, cancellationToken);
                    }
                    finally
                    {
                        // Release the slot even if the HTTP request fails
                        semaphore.Release();
                    }
                });

                var stories = await Task.WhenAll(tasks);

                var rankedStories = stories
                    .Where(story => story != null)
                    .Select(story => story!)
                    .OrderByDescending(story => story.Score)
                    .ToList();

                // Cache the ranked stories for one minute to reduce external API traffic
                _cache.Set(RankedStoriesCacheKey, rankedStories, TimeSpan.FromMinutes(1));

                return rankedStories.Take(n).ToList();
            }
            finally
            {
                // Ensure only one request refreshes the cache at a time
                _refreshLock.Release();
            }
        }

        private static async Task<StoryResponse?> GetStoryAsync(HttpClient client, int id,CancellationToken cancellationToken)
        {
            var item = await client.GetFromJsonAsync<HackerNewsItem>($"v0/item/{id}.json", cancellationToken);

            if (item == null)
            {
                return null;
            }

            return new StoryResponse
            {
                Title = item.Title,
                Uri = item.Url,
                PostedBy = item.By,
                Time = DateTimeOffset.FromUnixTimeSeconds(item.Time),
                Score = item.Score,
                CommentCount = item.Descendants
            };
        }
    }
}