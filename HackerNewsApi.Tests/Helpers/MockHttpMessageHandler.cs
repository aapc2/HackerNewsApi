using System.Net;
using System.Text;

namespace HackerNewsApi.Tests.Helpers
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _responses;

        public MockHttpMessageHandler(Dictionary<string, string> responses)
        {
            _responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.ToString();

            if (_responses.TryGetValue(url, out var json))
            {
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}