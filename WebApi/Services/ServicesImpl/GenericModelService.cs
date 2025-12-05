using Shared.Dtos.DtosImpl;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services.ServicesImpl
{
    // Add 'class' constraint to TModel to ensure it is a reference type
    public class GenericModelService<TModel, TSearchDto> : IModelService<TModel, TSearchDto>
        where TModel : class, IModel
        where TSearchDto : ISearchDto
    {
        private readonly DatabaseContext _db;
        public GenericModelService(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<TModel> Create(TModel dto)
        {
            _db.Set<TModel>().Add(dto);
            await _db.SaveChangesAsync();
            return dto;
        }

        public async Task Delete(int id)
        {
            var entity = await _db.Set<TModel>().FindAsync(id);
            if (entity != null)
            {
                _db.Set<TModel>().Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<TModel?> Read(int id)
        {
            return await _db.Set<TModel>().FindAsync(id);
        }

        public async Task<List<TModel>> ReadAll()
        {
            return await _db.Set<TModel>().ToListAsync();
        }

        public async Task<PaginationDto<TModel>> Search(TSearchDto search)
        {
            var query = _db.Set<TModel>().AsQueryable();
            int page = search.Page > 0 ? search.Page : 1;
            int pageSize = search.PageSize > 0 ? search.PageSize : 10;
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginationDto<TModel>
            {
                Data = items,
                TotalItems = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            };
        }

        public async Task<TModel> Update(TModel dto)
        {
            _db.Set<TModel>().Update(dto);
            await _db.SaveChangesAsync();
            return dto;
        }
    }
}
