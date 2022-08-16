namespace SuggestionClassLibrary.DataAccess;

public interface ISuggestionData
{
    Task CreateSuggestion(SuggestionModel suggestion);
    Task<List<SuggestionModel>> GetAllSuggestions();
    Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval();
    Task<List<SuggestionModel>> GetApprovedSuggestions();
    Task<SuggestionModel> GetSuggestion(string id);
    Task<List<SuggestionModel>> GetUsersSuggestion(string userId);
    Task UpdateSuggestion(SuggestionModel suggestion);
    Task UpvoteSuggestion(string suggestionId, string userId);
}