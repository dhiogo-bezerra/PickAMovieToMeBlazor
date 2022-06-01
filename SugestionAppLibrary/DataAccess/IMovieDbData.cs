
namespace MozifAppLibrary.DataAccess
{
    public interface IMovieDbData
    {
        Task AddListToMovie(string movieId, BasicMovieListModel movieList);
        Task CreateMovie(MovieDbModel movie);
        Task<List<MovieDbModel>> GetAllMovies();
        Task<MovieDbModel> GetMovieByImdbId(string imdbId);
        Task<List<MovieDbModel>> GetRandomMovie(int count);
        Task<MovieDbModel> GetRandonMovie();
        Task UpdateMovie(MovieDbModel movie);
    }
}