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

        public async Task<PaginationDto<SaveChickenAction>> SearchAsync(
            SaveChickenActionSearch search,
            params Expression<Func<SaveChickenAction, object?>>[] includes)
        {
            var query = _db.Set<SaveChickenAction>().AsQueryable();

            foreach (var include in includes)
                query = query.Include(include);

            // Filter by Ids
            if (search.Ids != null && search.Ids.Any())
                query = query.Where(x => search.Ids.Contains(x.Id));

            // SearchTerm (preferred)
            if (!string.IsNullOrEmpty(search.SearchTerm))
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search.SearchTerm}%"));

            // Title (backward compat) — only applied if SearchTerm wasn't already used
            if (!string.IsNullOrEmpty(search.Title) && string.IsNullOrEmpty(search.SearchTerm))
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search.Title}%"));

            if (search.IsArchived.HasValue)
                query = query.Where(x => x.IsArchived == search.IsArchived.Value);

            int page = search.Page > 0 ? search.Page : 1;
            int pageSize = search.PageSize > 0 ? search.PageSize : 10;

            if (!string.IsNullOrEmpty(search.SortBy) && search.SortBy == "Dates")
            {
                // In-memory sort and page for Dates
                bool descending = search.SortDescending ?? false;
                var allItems = descending
                    ? query.AsEnumerable().OrderByDescending(x => x.Dates != null && x.Dates.Any() ? x.Dates.Min() : DateOnly.MaxValue).ToList()
                    : query.AsEnumerable().OrderBy(x => x.Dates != null && x.Dates.Any() ? x.Dates.Min() : DateOnly.MaxValue).ToList();
                var total = allItems.Count;
                var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return new PaginationDto<SaveChickenAction>
                {
                    Data = items,
                    TotalItems = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                };
            }
            else
            {
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
}
