using Microsoft.EntityFrameworkCore;
using Shared.Dtos.DtosImpl;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebApi.Models.ModelsImpl;

namespace Services.ServicesImpl
{
    public class SaveChickenActionSearcher : IModelSearcher<SaveChickenAction, SaveChickenActionSearch>
    {
        private readonly DatabaseContext _db;
        public SaveChickenActionSearcher(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<PaginationDto<SaveChickenAction>> SearchAsync(SaveChickenActionSearch search, params Expression<Func<SaveChickenAction, object?>>[] includes)
        {
            var query = _db.Set<SaveChickenAction>().AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Filter by Ids
            if (search.Ids != null && search.Ids.Any())
                query = query.Where(x => search.Ids.Contains(x.Id));

            // Use SearchTerm from ISearchDto for consistency
            if (!string.IsNullOrEmpty(search.SearchTerm))
            {
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search.SearchTerm}%"));
            }

            // Also support Title for backward compatibility
            if (!string.IsNullOrEmpty(search.Title))
            {
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search.Title}%"));
            }

            // Sorting
            if (!string.IsNullOrEmpty(search.SortBy))
            {
                bool descending = search.SortDescending ?? false;
                string sortExpression = $"{search.SortBy} {(descending ? "descending" : "ascending")}";
                query = query.OrderBy(sortExpression);
            }

            // Pagination
            int page = search.Page > 0 ? search.Page : 1;
            int pageSize = search.PageSize > 0 ? search.PageSize : 10;
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginationDto<SaveChickenAction>
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
