using Microsoft.Extensions.Caching.Memory;

namespace SugestionAppLibrary.DataAccess;

public class MongoGenreData : IGenreData
{
    private readonly IMemoryCache _cache;
    private readonly IMongoCollection<GenreModel> _genre;
    private const string cacheName = "GenreData";

    public MongoGenreData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _genre = db.GenreCollection;
    }

    public async Task<List<GenreModel>> GetAllGenre()
    {
        var output = _cache.Get<List<GenreModel>>(cacheName);

        if (output is null || output.Count == 0)
        {
            var results = await _genre.FindAsync(_ => true);
            output = results.ToList();

            _cache.Set(cacheName, output, TimeSpan.FromDays(1));
        }
        return output;
    }

    public Task CreateGenre(GenreModel genre)
    {
        return _genre.InsertOneAsync(genre);
    }
}
