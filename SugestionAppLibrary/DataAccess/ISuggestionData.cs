
namespace SugestionAppLibrary.DataAccess
{
    public interface ISuggestionData
    {
        Task CreateSuggestion(SuggestionModel suggestion);
        Task<List<SuggestionModel>> GetAllApprovedSuggestions();
        Task<List<SuggestionModel>> GetAllSuggestion();
        Task<SuggestionModel> GetSuggestion(string id);
        Task<List<SuggestionModel>> GetSuggestionsWaitingForApproval();
        Task<List<SuggestionModel>> GetUserSugestion(string userId);
        Task UpdateSuggestion(SuggestionModel suggestion);
        Task UpvoteSuggestion(string suggestionId, string userID);
    }
}