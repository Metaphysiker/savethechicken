using WebApi.Services.ServicesImpl;
using Shared.Dtos.DtosImpl;

namespace WebApi.Factories.FactoriesImpl
{
    public class GenericModelServiceFactory
    {
        private readonly DatabaseContext _db;
        private readonly ModelSearchFactory _modelSearchFactory;
        public GenericModelServiceFactory(DatabaseContext db, ModelSearchFactory modelSearchFactory)
        {
            _db = db;
            _modelSearchFactory = modelSearchFactory;
        }

        public GenericModelService<TModel, TSearchDto> Create<TModel, TSearchDto>()
            where TModel : class, IModel
            where TSearchDto : ISearchDto
        {
            return new GenericModelService<TModel, TSearchDto>(_db, _modelSearchFactory);
        }
    }
}
