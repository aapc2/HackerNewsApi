using HackerNewsApi.Models;
using HackerNewsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;

        public StoriesController(IHackerNewsService hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
        }

        [HttpGet("best")]
        public async Task<ActionResult<List<StoryResponse>>>
            GetBestStories([FromQuery] int n, CancellationToken cancellationToken)
        {
            if (n <= 0)
            {
                return BadRequest("The value of n must be greater than 0.");
            }

            if (n > 500)
            {
                return BadRequest("The value of n cannot be greater than 500.");
            }

            var stories = await _hackerNewsService.GetBestStoriesAsync(n, cancellationToken);

            return Ok(stories);
        }
    }
}