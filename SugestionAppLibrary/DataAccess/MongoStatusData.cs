using Microsoft.Extensions.Caching.Memory;

namespace MozifAppLibrary.DataAccess;

public class MongoStatusData : IStatusData
{
    private readonly IMemoryCache _cache;
    private readonly IMongoCollection<StatusModel> _statuses;
    private const string cacheName = "StatusData";

    public MongoStatusData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _statuses = db.StatusCollection;
    }

    public async Task<List<StatusModel>> GetAllStatus()
    {
        var output = _cache.Get<List<StatusModel>>(cacheName);

        if (output is null || output.Count == 0)
        {
            var results = await _statuses.FindAsync(_ => true);
            output = results.ToList();

            _cache.Set(cacheName, output, TimeSpan.FromDays(1));
        }
        return output;
    }

    public Task CreateStatus(StatusModel category)
    {
        return _statuses.InsertOneAsync(category);
    }
}
