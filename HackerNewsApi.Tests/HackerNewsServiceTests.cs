using HackerNewsApi.Services;
using HackerNewsApi.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace HackerNewsApi.Tests
{
    public class HackerNewsServiceTests
    {
        [Fact]
        public async Task GetBestStoriesAsync_ReturnsStoriesOrderedByScore()
        {
            var responses = new Dictionary<string, string>
            {
                {
                    "https://hacker-news.firebaseio.com/v0/beststories.json",
                    "[101,102,103,104]"
                },
                {
                    "https://hacker-news.firebaseio.com/v0/item/101.json",
                    """
                    {
                        "id": 101,
                        "title": "Run Qwen 3.8 Flash Next on consumer hardware",
                        "url": "https://github.com/Niko1221/Strata",
                        "by": "snehesht",
                        "time": 1791118313,
                        "score": 916,
                        "descendants": 417
                    }
                    """
                },
                {
                    "https://hacker-news.firebaseio.com/v0/item/102.json",
                    """
                    {
                        "id": 102,
                        "title": "Turn off Apple Intelligence on macOS 27",
                        "url": "https://github.com/omlahore/RemoveMacAI",
                        "by": "privacyisntdead",
                        "time": 1791142945,
                        "score": 758,
                        "descendants": 533
                    }
                    """
                },
                {
                    "https://hacker-news.firebaseio.com/v0/item/103.json",
                    """
                    {
                        "id": 103,
                        "title": "Web Search API",
                        "url": "https://developers.cloudflare.com/changelog/post/2026-10-02-introducing-web-search-api/",
                        "by": "tosh",
                        "time": 1791197226,
                        "score": 544,
                        "descendants": 248
                    }
                    """
                },
                {
                    "https://hacker-news.firebaseio.com/v0/item/104.json",
                    """
                    {
                        "id": 104,
                        "title": "Plain text is still one of the best technologies we have",
                        "url": "https://deadparrotbbs.com/why-plain-text-is-still-one-of-the-best-technologies-we-have/",
                        "by": "speckx",
                        "time": 1791226440,
                        "score": 191,
                        "descendants": 104
                    }
                    """
                }
            };

            var handler = new MockHttpMessageHandler(responses);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            var factoryMock = new Mock<IHttpClientFactory>();

            factoryMock.Setup(factory => factory.CreateClient("HackerNews")).Returns(httpClient);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new HackerNewsService(factoryMock.Object, cache);
            var result = await service.GetBestStoriesAsync(3, CancellationToken.None);

            Assert.Equal(3, result.Count);
            Assert.Equal(916, result[0].Score);
            Assert.Equal(758, result[1].Score);
            Assert.Equal(544, result[2].Score);
            Assert.Equal("Run Qwen 3.8 Flash Next on consumer hardware", result[0].Title);
            Assert.Equal("https://github.com/Niko1221/Strata", result[0].Uri);
            Assert.Equal("snehesht", result[0].PostedBy);
            Assert.Equal(417, result[0].CommentCount);
        }
    }
}