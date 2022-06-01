using Microsoft.Extensions.Caching.Memory;
using MozifAppLibrary.Helpers;

namespace MozifAppLibrary.DataAccess;

public class MongoMovieDb : IMovieDbData
{
    private readonly IMemoryCache _cache;
    private readonly IDbConnection _db;
    private readonly IMongoCollection<MovieDbModel> _movie;
    private const string cacheName = "Movies";

    public MongoMovieDb(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _movie = db.MovieDbCollection;
        _db = db;
    }

    public async Task<List<MovieDbModel>> GetAllMovies()
    {
        var output = _cache.Get<List<MovieDbModel>>(cacheName);

        if (output is null || output.Count == 0)
        {
            var results = await _movie.FindAsync(_ => true);
            output = results.ToList();

            _cache.Set(cacheName, output, TimeSpan.FromDays(1));
        }
        return output;
    }

    public Task CreateMovie(MovieDbModel movie)
    {
        return _movie.InsertOneAsync(movie);
    }

    public async Task<MovieDbModel> GetMovieByImdbId(string imdbId)
    {
        try
        {
            var result = await _movie.FindAsync(u => u.Const == imdbId);
            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            return null;
        }
  
    }

    public async Task AddListToMovie(string movieId, BasicMovieListModel movieList)
    {
        var client = _db.Client;

        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var movieInTransaction = db.GetCollection<MovieDbModel>(_db.MovieDbCollectionName);

            var movie = (await movieInTransaction.FindAsync(s => s.Id == movieId)).First();

            movie.MemberOf.Add((movieList));
           
            await movieInTransaction.ReplaceOneAsync(session, s => s.Id == movieId, movie);

            var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);

            await session.CommitTransactionAsync();

            //Remove cache so the next pool on the database create a new cache
            //_cache.Remove(cacheName);

        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
    public async Task<MovieDbModel> GetRandonMovie()
    {
        try
        {
            var movieList = await GetAllMovies();
            var movie = movieList.Random();
            return movie;
        }
        catch (Exception ex)
        {
            return null;
        }

    }

    public async Task<List<MovieDbModel>> GetRandomMovie(int count)
    {
        try
        {
            var movieList = await GetAllMovies();
            var movie = movieList.GetRandomElements(count);
            return movie;
        }
        catch (Exception ex)
        {
            return null;
        }

    }

    public Task UpdateMovie(MovieDbModel movie)
    {
        var filter = Builders<MovieDbModel>.Filter.Eq("Id", movie.Id);
        return _movie.ReplaceOneAsync(filter, movie, new ReplaceOptions { IsUpsert = true });
    }
}
