namespace SuggestionClassLibrary.Models;

public class UserModel
{
    // Tells us that this is an identifier.
    [BsonId]

    // Tells us the MongoDb saved it as ObjectId
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ObjectIdentifier { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string EmailAddress { get; set; }

    public List<BasicSuggestionModel> AuthorSuggestions { get; set; } = new();
    public List<BasicSuggestionModel> VotedOnSuggestions { get; set; } = new();
}
