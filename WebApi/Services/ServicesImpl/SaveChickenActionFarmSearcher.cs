using Shared.Dtos.DtosImpl;
using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;
using Microsoft.EntityFrameworkCore; // Fixed: Provides Include, CountAsync, ToListAsync
using System.Linq.Dynamic.Core;     // Fixed: Provides string-based OrderBy

namespace Services.ServicesImpl
{
    public class SaveChickenActionFarmSearcher : IModelSearcher<SaveChickenActionFarm, SaveChickenActionFarmSearch>
    {
        private readonly DatabaseContext _db;
        public SaveChickenActionFarmSearcher(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<PaginationDto<SaveChickenActionFarm>> SearchAsync(
            SaveChickenActionFarmSearch search,
            params Expression<Func<SaveChickenActionFarm, object?>>[] includes)
        {
            var query = _db.Set<SaveChickenActionFarm>().AsQueryable();

            foreach (var include in includes)
                query = query.Include(include);

            // Filter by Ids
            if (search.Ids != null && search.Ids.Any())
                query = query.Where(x => search.Ids.Contains(x.Id));

            int page = search.Page > 0 ? search.Page : 1;
            int pageSize = search.PageSize > 0 ? search.PageSize : 10;

            // Use IQueryable for everything else
            if (!string.IsNullOrEmpty(search.SortBy))
            {
                bool descending = search.SortDescending ?? false;
                query = query.OrderBy($"{search.SortBy} {(descending ? "descending" : "ascending")}");
            }

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationDto<SaveChickenActionFarm>
            {
                Data = items,
                TotalItems = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            };

        }
    }
}