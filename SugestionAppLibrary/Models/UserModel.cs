
namespace MozifAppLibrary.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ObjectIdentifier { get; set; }
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public List<BasicSuggestionModel> AuthoredSuggestion { get; set; } = new();
        public List<BasicSuggestionModel> VotedOnSuggestion { get; set; } = new();
    }
}
