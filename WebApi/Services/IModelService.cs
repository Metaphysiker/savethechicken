using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Dtos.DtosImpl;
using System.Linq.Expressions;

namespace WebApi.Services
{
    public interface IModelService<ModelT, SearchT> where ModelT : class, IModel where SearchT : ISearchDto
    {
        public Task<List<ModelT>> ReadAll(params Expression<Func<ModelT, object?>>[] includes);
        public Task<ModelT?> Read(int id, params Expression<Func<ModelT, object?>>[] includes);
        public Task<ModelT> Create([FromBody] ModelT dto, params Expression<Func<ModelT, object?>>[] includes);
        public Task<ModelT> Update([FromBody] ModelT dto, params Expression<Func<ModelT, object?>>[] includes);
        public Task Delete(int id);
        public Task<PaginationDto<ModelT>> Search([FromBody] SearchT search, params Expression<Func<ModelT, object?>>[] includes);
    }
}
