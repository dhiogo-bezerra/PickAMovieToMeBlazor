
namespace SugestionAppLibrary.DataAccess
{
    public interface IStatusData
    {
        Task CreateStatus(StatusModel category);
        Task<List<StatusModel>> GetAllStatus();
    }
}