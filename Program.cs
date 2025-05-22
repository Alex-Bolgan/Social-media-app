using System.Data;
using Dapper;
using Npgsql;
using Social_media_app;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
builder.Services.AddScoped<PostService>();

var app = builder.Build();

app.MapPost("/users", async (User user, IDbConnection db) =>
{
    var id = await db.ExecuteScalarAsync<int>(
        "INSERT INTO users (username) VALUES (@Username) RETURNING id", user);
    return Results.Created($"/users/{id}", new { id });
});

app.MapGet("/users/{id}", async (int id, IDbConnection db) =>
{
    var user = await db.QuerySingleOrDefaultAsync<User>(
        "SELECT * FROM users WHERE id = @id", new { id });
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

app.MapPost("/posts", async (Post post, PostService postService) =>
{
    var id = postService.CreatePostAsync(post);
    return Results.Created($"/posts/{id}", new { id });
});

app.MapGet("/posts", async (PostService postService) =>
{
    var posts = postService.GetAllPostsAsync();
    return Results.Ok(posts);
});

app.MapPost("/follow", async (Follow follow, IDbConnection db) =>
{
    var rows = await db.ExecuteAsync(
        "INSERT INTO follows (follower_id, followee_id) VALUES (@FollowerId, @FolloweeId) ON CONFLICT DO NOTHING", follow);
    return Results.Ok(new { success = rows > 0 });
});

app.MapPost("/like", async (Like like, IDbConnection db) =>
{
    var rows = await db.ExecuteAsync(
        "INSERT INTO likes (user_id, post_id) VALUES (@UserId, @PostId) ON CONFLICT DO NOTHING", like);
    return Results.Ok(new { success = rows > 0 });
});

app.Run();

public record User(int Id, string Username);

public record Post(int Id, string Title, string Body, int AuthorId, string? AuthorName = null);

public record Follow(int FollowerId, int FolloweeId);

public record Like(int UserId, int PostId);
