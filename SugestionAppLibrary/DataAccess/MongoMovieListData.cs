using Microsoft.Extensions.Caching.Memory;

namespace SugestionAppLibrary.DataAccess;

public class MongoMovieListData : IMovieListData
{
    private readonly IMemoryCache _cache;
    private readonly IDbConnection _db;
    private readonly IMongoCollection<MovieListModel> _lists;
    private const string cacheName = "MovieList";

    public MongoMovieListData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _lists = db.MovieListCollection;
        _db = db;
    }

    public async Task<List<MovieListModel>> GetAllLists()
    {
        try
        {
            var output = _cache.Get<List<MovieListModel>>(cacheName);

            if (output is null || output.Count == 0)
            {
                var results = await _lists.FindAsync(_ => true);
                output = results.ToList();

                _cache.Set(cacheName, output, TimeSpan.FromDays(1));
            }
            return output;
        }
        catch (Exception ex)
        {
            return null;
        }
       
    }

    public Task CreateList(MovieListModel list)
    {
        return _lists.InsertOneAsync(list);
    }
}
