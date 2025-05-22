namespace Social_media_app
{
    using System.Data;
    using Dapper;

    public class PostService
    {
        private readonly IDbConnection _db;

        public PostService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreatePostAsync(Post post)
        {
            return await _db.ExecuteScalarAsync<int>(
                "INSERT INTO posts (title, body, author_id) VALUES (@Title, @Body, @AuthorId) RETURNING id", post);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _db.QueryAsync<Post>(
                @"SELECT p.*, u.username AS AuthorName
              FROM posts p
              JOIN users u ON p.author_id = u.id");
        }
    }

}
