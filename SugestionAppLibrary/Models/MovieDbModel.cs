using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugestionAppLibrary.Models;

public class MovieDbModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public int TmdbId { get; set; }
    public Int32 Position { get; set; }
    public string Const { get; set; }
    public string Title { get; set; }
    public string Overview { get; set; }
    public double ImDbRating { get; set; }
    public double VoteAverage { get; set; }
    public long RuntimeMins { get; set; }
    public Int16 Year { get; set; }
    public string Genres { get; set; }
    public string ReleaseDate { get; set; }        
    public string Directors { get; set; }
    public string PosterPath { get; set; }
    public List<BasicMovieListModel> MemberOf { get; set; } = new();


}
