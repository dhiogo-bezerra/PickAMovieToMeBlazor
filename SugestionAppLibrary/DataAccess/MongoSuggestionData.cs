
using Microsoft.Extensions.Caching.Memory;

namespace SugestionAppLibrary.DataAccess;

public class MongoSuggestionData : ISuggestionData
{
    private readonly IDbConnection _db;
    private readonly IMemoryCache _cache;
    private readonly IUserData _userData;
    private readonly IMongoCollection<SuggestionModel> _suggestions;
    private const string cacheName = "SuggestionData";

    public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
    {
        _db = db;
        _userData = userData;
        _cache = cache;
        _suggestions = db.SugestionCollection;
    }

    public async Task<List<SuggestionModel>> GetAllSuggestion()
    {
        var output = _cache.Get<List<SuggestionModel>>(cacheName);
        if (output == null)
        {
            var results = await _suggestions.FindAsync(s => s.Archived == false);
            output = results.ToList();

            _cache.Set(cacheName, output, TimeSpan.FromMinutes(1));
        }
        return output;
    }

    public async Task<List<SuggestionModel>> GetUserSugestion(string userId)
    {
        var output = _cache.Get<List<SuggestionModel>>(userId);
        if (output == null)
        {
            var results = await _suggestions.FindAsync(s => s.Author.Id == userId);
            output = results.ToList();

            _cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }
        return output;
    }

    public async Task<List<SuggestionModel>> GetAllApprovedSuggestions()
    {
        var output = await GetAllSuggestion();
        return output.Where(x => x.ApprovedForRelease).ToList();
    }

    public async Task<SuggestionModel> GetSuggestion(string id)
    {
        var results = await _suggestions.FindAsync(s => s.Id == id);
        return results.FirstOrDefault();
    }

    public async Task<List<SuggestionModel>> GetSuggestionsWaitingForApproval()
    {
        var output = await GetAllSuggestion();
        return output.Where(x => x.ApprovedForRelease == false && x.Rejected == false).ToList();
    }

    public async Task UpdateSuggestion(SuggestionModel suggestion)
    {
        await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
        _cache.Remove(cacheName);
    }

    public async Task UpvoteSuggestion(string suggestionId, string userID)
    {
        var client = _db.Client;

        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            var suggestion = (await suggestionsInTransaction.FindAsync(s => s.Id == suggestionId)).First();

            //If userId already in the Hashset the return is false, so we remove the userId from the upvote list instead
            bool isUpvote = suggestion.UserVotes.Add(userID);
            if (isUpvote == false)
            {
                suggestion.UserVotes.Remove(userID);
            }

            await suggestionsInTransaction.ReplaceOneAsync(session,s => s.Id == suggestionId, suggestion);

            var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);

            var user = await _userData.GetUser(userID);


            //If is a upvote we add the basicSuggestion (Id and title) to the users document. If is a removal of a upvote we delete the suggestion from the users document
            if (isUpvote)
            {
                //The basicSuggestionModel class has a constructor with a full sugestion model as an argument
                user.VotedOnSuggestion.Add(new BasicSuggestionModel(suggestion));
            }
            else
            {
                var suggestionToRemove = user.VotedOnSuggestion.Where(s => s.Id == suggestionId).First();
                user.VotedOnSuggestion.Remove(suggestionToRemove);
            }

            await usersInTransaction.ReplaceOneAsync(session,u => u.Id == userID, user);

            await session.CommitTransactionAsync();

            //Remove cache so the next pool on the database create a new cache
            _cache.Remove(cacheName);

        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task CreateSuggestion(SuggestionModel suggestion)
    {
        var client = _db.Client;
        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            await suggestionsInTransaction.InsertOneAsync(suggestion);

            var usersInTransatcion = db.GetCollection<UserModel>(_db.UserCollectionName);
            var user = await _userData.GetUser(suggestion.Author.Id);

            user.AuthoredSuggestion.Add(new BasicSuggestionModel(suggestion));

            await usersInTransatcion.ReplaceOneAsync(session, u => u.Id == user.Id, user);

            await session.CommitTransactionAsync();

            //The cache is not destroyed because the suggestion will apear only after a admin manually approves
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}
