using Services.ServicesImpl;
using Shared.Dtos.DtosImpl;
using WebApi.Models.ModelsImpl;

public class ModelSearchFactory
{

    private readonly DatabaseContext _db;

    public ModelSearchFactory(DatabaseContext db)
    {
        _db = db;
    }

    public IModelSearcher<ModelT, SearchDtoT> Create<ModelT, SearchDtoT>()
        where ModelT : IModel
        where SearchDtoT : ISearchDto
    {
        if (typeof(ModelT) == typeof(SaveChickenRequest) && typeof(SearchDtoT) == typeof(SaveChickenRequestSearch))
        {
            return (IModelSearcher<ModelT, SearchDtoT>)new SaveChickenRequestSearcher(_db);
        }

        if (typeof(ModelT) == typeof(SaveChickenDriveRequest) && typeof(SearchDtoT) == typeof(SaveChickenDriveRequestSearch))
        {
            return (IModelSearcher<ModelT, SearchDtoT>)new SaveChickenDriveRequestSearcher(_db);
        }

        if( typeof(ModelT) ==  typeof(SaveChickenAction) && typeof(SearchDtoT) == typeof(SaveChickenActionSearch))
        {
            return (IModelSearcher<ModelT, SearchDtoT>)new SaveChickenActionSearcher(_db);
        }

        if (typeof(ModelT) == typeof(Farm) && typeof(SearchDtoT) == typeof(FarmSearch))
        {
            return (IModelSearcher<ModelT, SearchDtoT>)new FarmSearcher(_db);
        }

        if (typeof(ModelT) == typeof(Person) && typeof(SearchDtoT) == typeof(PersonSearch))
        {
            return (IModelSearcher<ModelT, SearchDtoT>)new PersonSearcher(_db);
        }

        throw new NotImplementedException($"No searcher implemented for model type {typeof(ModelT)} and search dto type {typeof(SearchDtoT)}");
    }
}
