using MongoDB.Driver;

namespace SugestionAppLibrary.DataAccess
{
    public interface IDbConnection
    {
        IMongoCollection<CategoryModel> CategoryCollection { get; }
        string CategoryCollectionName { get; }
        MongoClient Client { get; }
        string DbName { get; }
        IMongoCollection<StatusModel> StatusCollection { get; }
        string StatusCollectionName { get; }
        IMongoCollection<SuggestionModel> SugestionCollection { get; }
        string SuggestionCollectionName { get; }
        IMongoCollection<UserModel> UserCollection { get; }
        string UserCollectionName { get; }
        IMongoCollection<MovieListModel> MovieListCollection { get; }
        IMongoCollection<MovieDbModel> MovieDbCollection { get; }
        IMongoCollection<GenreModel> GenreCollection { get; }
        string MovieDbCollectionName { get; }
    }
}