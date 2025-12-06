using Shared.Dtos.DtosImpl;

public  interface IModelSearcher<ModelT, SearchDtoT>
    where ModelT : IModel
    where SearchDtoT : ISearchDto
{
    Task<PaginationDto<ModelT>> SearchAsync(SearchDtoT searchDto);
}
