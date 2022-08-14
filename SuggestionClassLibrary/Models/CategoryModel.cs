namespace SuggestionClassLibrary.Models;

public class CategoryModel
{
    // Tells us that this is an identifier.
    [BsonId]

    // Tells us the MongoDb saved it as ObjectId
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
}
