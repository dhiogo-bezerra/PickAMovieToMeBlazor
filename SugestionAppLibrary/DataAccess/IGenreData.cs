
namespace MozifAppLibrary.DataAccess
{
    public interface IGenreData
    {
        Task CreateGenre(GenreModel genre);
        Task<List<GenreModel>> GetAllGenre();
    }
}