# HackerNewsApi

ASP.NET Core REST API that retrieves the top `n` Hacker News stories ordered by score.

## Overview

This project implements a RESTful API using ASP.NET Core and .NET 8.

The API retrieves the current list of best stories from Hacker News, obtains the details of each story, orders them by score in descending order, and returns the requested number of results.

Example endpoint:

```http
GET /api/stories/best?n=10
```
Example response:
```Json
[
  {
    "title": "Example story",
    "uri": "https://example.com",
    "postedBy": "user",
    "time": "2026-10-06T08:15:00+00:00",
    "score": 950,
    "commentCount": 210
  }
]
```
## Technologies

- .NET 8
- ASP.NET Core Web API
- xUnit
- Moq
- Swagger / OpenAPI
- IMemoryCache
- HttpClientFactory
- LINQ
  
## Project Structure
```
HackerNewsApi
├── Controllers
│   └── StoriesController.cs
├── Models
│   ├── HackerNewsItem.cs
│   └── StoryResponse.cs
├── Services
│   ├── IHackerNewsService.cs
│   └── HackerNewsService.cs
└── Program.cs

HackerNewsApi.Tests
├── Helpers
│   └── MockHttpMessageHandler.cs
├── HackerNewsServiceTests.cs
└── StoriesControllerTests.cs
```

## Running the Application
### Requirements
- .NET 8 SDK
- Visual Studio 2022 or another compatible .NET development environment

### Run from Visual Studio

1. Open HackerNewsApi.sln
2. Set HackerNewsApi as the startup project
3. Run the application
4. Swagger will be available in the browser in Development mode
5. Execute:
```http
GET /api/Stories/best?n=10
```

### Run from the command line

From the solution directory:
```Bash
dotnet restore
dotnet build
dotnet run --project HackerNewsApi/HackerNewsApi.csproj
```
The exact local port is determined by the development configuration.

### Running the Tests

From the solution directory:
```Bash
dotnet test
```

The tests cover:
- invalid values of n
- values greater than the supported maximum
- successful controller responses
- mapping between Hacker News data and the API response model
- limiting the response to the requested number of stories
- ordering stories by score in descending order
 
External Hacker News calls are mocked during unit testing.

## Implementation Details

### Hacker News API
The solution uses the following Hacker News endpoints:
```http
https://hacker-news.firebaseio.com/v0/beststories.json
```
to retrieve story IDs, and:
```http
https://hacker-news.firebaseio.com/v0/item/{id}.json
```
to retrieve the details of each story.
The final ordering is performed by this API using the score property rather than relying on the order returned by Hacker News.

### Asynchronous Processing
External HTTP operations use async and await so that request threads are not blocked while waiting for network responses.
Task.WhenAll is used to execute multiple story requests concurrently.

### Bounded Concurrency
A SemaphoreSlim limits the maximum number of simultaneous requests sent to Hacker News.
This improves response time while preventing the application from sending an excessive number of requests to the external API at once.

### Caching
The ranked story list is stored temporarily using IMemoryCache.

The cache reduces repeated calls to Hacker News when multiple requests ask for the same or overlapping data.

The current cache duration is one minute. This represents a trade-off between data freshness and reducing external API traffic.

A synchronization lock is also used during cache refresh so that multiple simultaneous requests do not all refresh the cache at the same time.

### Cancellation
CancellationToken is propagated through asynchronous operations so that work can be cancelled if the original HTTP request is aborted.

### Assumptions
- n must be greater than zero
- n is limited to 500 because Hacker News exposes up to 500 best stories
- A one-minute cache lifetime is considered acceptable for this exercise
- Temporary differences between cached data and the latest Hacker News scores are acceptable during the cache lifetime
- Hacker News is treated as the external source of truth
- Authentication is not required because the Hacker News API used by this exercise is public

## Possible Improvements
Given additional time and a real deployment environment, the following improvements could be considered:

### Environment-specific configuration
Provide separate settings for:
- DEV
- UAT
- PROD
  
Environment-specific URLs and non-sensitive settings could be stored in environment variables or deployment configuration.

### Secrets Management
If credentials, API keys or tokens were required, they should not be stored in source control.

For local development, .NET User Secrets or environment variables could be used.

For production, a secure secret manager such as AWS Secrets Manager or AWS Systems Manager Parameter Store could be used.

### CI/CD
A GitHub Actions workflow could be added to:
- restore dependencies
- build the solution
- run unit tests
- publish the application
- deploy to DEV, UAT or PROD
  
GitHub Environments could be used to keep environment-specific variables and secrets separate.

### Logging
Structured logging could be added using ILogger<T> with appropriate log levels such as:
- Information
- Warning
- Error
  
This would improve troubleshooting and operational visibility.

### Monitoring and Observability
A production deployment could integrate an observability platform such as New Relic or AWS CloudWatch for:
- centralized logs
- metrics
- traces
- error monitoring
- application performance monitoring

### Distributed Caching
IMemoryCache is sufficient for this exercise and a single application instance.

For a distributed deployment with multiple instances, a distributed cache could be considered instead.

## Notes
The solution intentionally focuses on the requirements of the coding exercise without introducing infrastructure or deployment dependencies that were not specified.
