using HackerNewsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Cache
builder.Services.AddMemoryCache();

// Hacker News HttpClient
builder.Services.AddHttpClient("HackerNews", client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
});

// Application services
builder.Services.AddSingleton<IHackerNewsService,HackerNewsService>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();