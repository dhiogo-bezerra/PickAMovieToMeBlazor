namespace MozifAppLibrary.Models
{
    public class BasicMovieListModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }

        public BasicMovieListModel()
        {

        }

        public BasicMovieListModel(MovieListModel list)
        {
            Id= list.Id;
            Name = list.Name;
        }
    }
}
