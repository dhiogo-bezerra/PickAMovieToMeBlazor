

namespace SugestionAppLibrary.Models
{
    public class BasicAuthorModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string DisplayName { get; set; }

        public BasicAuthorModel()
        {

        }

        public BasicAuthorModel(UserModel user)
        {
            Id = user.Id;
            DisplayName = user.DisplayName;
        }
    }
}
