namespace SuggestionClassLibrary.Models;

public class SuggestionModel
{

    // Tells us that this is an identifier.
    [BsonId]

    // Tells us the MongoDb saved it as ObjectId
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Suggestion { get; set; }

    public string Description { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public CategoryModel Category { get; set; }

    public BasicUserModel Author { get; set; }

    public HashSet<string> UserVotes { get; set; } = new();

    public StatusModel SuggestionStatus { get; set; }

    public string AdminNotes { get; set; }

    public bool ApprovedForRelease { get; set; } = false;

    public bool Archived { get; set; } = false;

    public bool Rejected { get; set; } = false;

}
