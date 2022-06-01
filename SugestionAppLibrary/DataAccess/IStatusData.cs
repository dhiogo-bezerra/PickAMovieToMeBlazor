
namespace MozifAppLibrary.DataAccess
{
    public interface IStatusData
    {
        Task CreateStatus(StatusModel category);
        Task<List<StatusModel>> GetAllStatus();
    }
}