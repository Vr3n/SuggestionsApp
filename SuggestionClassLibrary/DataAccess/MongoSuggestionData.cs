using Microsoft.Extensions.Caching.Memory;

namespace SuggestionClassLibrary.DataAccess;

public class MongoSuggestionData : ISuggestionData
{
    private readonly IDbConnection _db;
    private readonly IUserData _userData;
    private readonly IMemoryCache _Cache;
    private readonly IMongoCollection<SuggestionModel> _suggestions;

    private readonly string cacheName = "suggestions";

    public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
    {
        _Cache = cache;
        _db = db;
        _userData = userData;
        _suggestions = db.SuggestionCollection;
    }

    public async Task<List<SuggestionModel>> GetAllSuggestions()
    {
        var output = _Cache.Get<List<SuggestionModel>>(cacheName);

        if (output is null)
        {
            var result = await _suggestions.FindAsync(s => s.Archived == false);
            output = result.ToList();

            _Cache.Set(cacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<SuggestionModel>> GetUsersSuggestion(string userId)
    {
        var output = _Cache.Get<List<SuggestionModel>>(userId);

        if (output is null)
        {
            var results = await _suggestions.FindAsync(s => s.Author.Id == userId);

            output = results.ToList();

            _Cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }

        return output;

    }

    public async Task<List<SuggestionModel>> GetApprovedSuggestions()
    {
        var output = await GetAllSuggestions();

        return output.Where(x => x.ApprovedForRelease).ToList();
    }

    public async Task<SuggestionModel> GetSuggestion(string id)
    {
        var result = await _suggestions.FindAsync(x => x.Id == id);
        return result.FirstOrDefault();
    }

    public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval()
    {
        var output = await GetAllSuggestions();
        return output.Where(x =>
            x.ApprovedForRelease == false
            && x.Rejected == false).ToList();
    }

    public async Task CreateSuggestion(SuggestionModel suggestion)
    {
        var client = _db.Client;

        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionsInTransaciton = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            await suggestionsInTransaciton.InsertOneAsync(suggestion);

            var usersInTransaciton = db.GetCollection<UserModel>(_db.UserCollectionName);
            var user = await _userData.GetUserAsync(suggestion.Author.Id);
            user.AuthorSuggestions.Add(new BasicSuggestionModel(suggestion));
            await usersInTransaciton.ReplaceOneAsync(u => u.Id == user.Id, user);

            await session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task UpdateSuggestion(SuggestionModel suggestion)
    {
        await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
        _Cache.Remove(cacheName);
    }

    public async Task UpvoteSuggestion(string suggestionId, string userId)
    {
        var client = _db.Client;

        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            var suggestion = (await suggestionsInTransaction.FindAsync(s => s.Id == suggestionId)).First();

            bool isUpvote = suggestion.UserVotes.Add(userId);
            if (isUpvote == false)
            {
                suggestion.UserVotes.Remove(userId);
            }

            await suggestionsInTransaction.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);

            var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);

            var user = await _userData.GetUserAsync(suggestion.Author.Id);

            if (isUpvote)
            {
                user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
            }
            else
            {
                var suggestionToRemove = user.VotedOnSuggestions.Where(s => s.Id == suggestionId).First();
                user.VotedOnSuggestions.Remove(suggestionToRemove);
            }

            await usersInTransaction.ReplaceOneAsync(u => u.Id == userId, user);

            await session.CommitTransactionAsync();

            _Cache.Remove(cacheName);

        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}
