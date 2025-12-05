using WebApi.Services.ServicesImpl;
using Shared.Dtos.DtosImpl;

namespace WebApi.Factories.FactoriesImpl
{
    public class GenericModelServiceFactory
    {
        private readonly DatabaseContext _db;
        public GenericModelServiceFactory(DatabaseContext db)
        {
            _db = db;
        }

        public GenericModelService<TModel, TSearchDto> Create<TModel, TSearchDto>()
            where TModel : class, IModel
            where TSearchDto : ISearchDto
        {
            return new GenericModelService<TModel, TSearchDto>(_db);
        }
    }
}
