
namespace SugestionAppLibrary.DataAccess
{
    public interface IMovieDbData
    {
        Task AddListToMovie(string movieId, BasicMovieListModel movieList);
        Task CreateMovie(MovieDbModel movie);
        Task<List<MovieDbModel>> GetAllMovies();
        Task<MovieDbModel> GetMovieByImdbId(string imdbId);
        Task<MovieDbModel> GetRandonMovie();
        Task UpdateMovie(MovieDbModel movie);
    }
}