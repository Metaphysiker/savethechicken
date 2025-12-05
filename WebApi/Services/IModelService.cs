using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Dtos.DtosImpl;

namespace WebApi.Services
{
    public interface IModelService<ModelT, SearchT> where ModelT : class, IModel where SearchT : ISearchDto
    {
        public Task<List<ModelT>> ReadAll();
        public Task<ModelT?> Read(int id);
        public Task<ModelT> Create([FromBody] ModelT dto);
        public Task<ModelT> Update([FromBody] ModelT dto);
        public Task Delete(int id);
        public Task<PaginationDto<ModelT>> Search([FromBody] SearchT search);
    }
}
