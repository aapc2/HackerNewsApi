using HackerNewsApi.Controllers;
using HackerNewsApi.Models;
using HackerNewsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HackerNewsApi.Tests
{
    public class StoriesControllerTests
    {
        [Fact]
        public async Task GetBestStories_ReturnsBadRequest_WhenNIsZero()
        {
            var serviceMock = new Mock<IHackerNewsService>();
            var controller = new StoriesController(serviceMock.Object);
            var result = await controller.GetBestStories(0, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetBestStories_ReturnsBadRequest_WhenNIsGreaterThan500()
        {
            var serviceMock = new Mock<IHackerNewsService>();
            var controller = new StoriesController(serviceMock.Object);
            var result = await controller.GetBestStories(501, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetBestStories_ReturnsOk_WithStories()
        {
            var expectedStories = new List<StoryResponse>
            {
                new StoryResponse
                {
                    Title = "Story 1",
                    Uri = "https://example.com/1",
                    PostedBy = "user1",
                    Time = DateTimeOffset.UtcNow,
                    Score = 100,
                    CommentCount = 10
                },
                new StoryResponse
                {
                    Title = "Story 2",
                    Uri = "https://example.com/2",
                    PostedBy = "user2",
                    Time = DateTimeOffset.UtcNow,
                    Score = 80,
                    CommentCount = 5
                }
            };

            var serviceMock = new Mock<IHackerNewsService>();

            serviceMock.Setup(service => service.GetBestStoriesAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(expectedStories);

            var controller = new StoriesController(serviceMock.Object);
            var result = await controller.GetBestStories(2, CancellationToken.None);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var stories = Assert.IsType<List<StoryResponse>>(okResult.Value);

            Assert.Equal(2, stories.Count);
            Assert.Equal("Story 1", stories[0].Title);
        }
    }
}