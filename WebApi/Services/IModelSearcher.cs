using Shared.Dtos.DtosImpl;
using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

public  interface IModelSearcher<ModelT, SearchDtoT>
    where ModelT : IModel
    where SearchDtoT : ISearchDto
{
    Task<PaginationDto<ModelT>> SearchAsync(SearchDtoT searchDto, params Expression<Func<ModelT, object?>>[] includes);
}
