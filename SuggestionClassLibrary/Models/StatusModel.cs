namespace SuggestionClassLibrary.Models;

public class StatusModel
{
    // Tells us that this is an identifier.
    [BsonId]

    // Tells us the MongoDb saved it as ObjectId
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string StatusName { get; set; }

    public string StatusDescription { get; set; }
}
