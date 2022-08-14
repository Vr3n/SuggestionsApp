using Microsoft.Extensions.Caching.Memory;

namespace SuggestionClassLibrary.DataAccess;

public class MongoCategoryData : ICategoryData
{
    private readonly IMemoryCache _Cache;
    private readonly IMongoCollection<CategoryModel> _categories;
    private const string cacheName = "CategoryData";

    public MongoCategoryData(IDbConnection db, IMemoryCache cache)
    {
        _Cache = cache;
        _categories = db.CategoryCollection;
    }

    public async Task<List<CategoryModel>> GetAllCategories()
    {

        var output = _Cache.Get<List<CategoryModel>>(cacheName);

        if (output is null)
        {
            var results = await _categories.FindAsync(_ => true);
            output = results.ToList();

            _Cache.Set(cacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public Task CreateCategory(CategoryModel category)
    {
        return _categories.InsertOneAsync(category);
    }
}
