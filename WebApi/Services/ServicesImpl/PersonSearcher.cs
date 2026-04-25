using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Shared.Dtos.DtosImpl;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebApi.Models.ModelsImpl;

namespace Services.ServicesImpl
{
    public class PersonSearcher : IModelSearcher<Person, PersonSearch>
    {
        private readonly DatabaseContext _db;
        public PersonSearcher(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<PaginationDto<Person>> SearchAsync(PersonSearch search, params Expression<Func<Person, object?>>[] includes)
        {
            var query = _db.Set<Person>().AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Filter by Ids
            if (search.Ids != null && search.Ids.Any())
                query = query.Where(x => search.Ids.Contains(x.Id));

            // Filter by IsBlacklisted
            if (search.IsBlacklisted.HasValue)
                query = query.Where(x => x.IsBlacklisted == search.IsBlacklisted.Value);

            // Filter by IsDriver
            if (search.IsDriver.HasValue)
                query = query.Where(x => x.IsDriver == search.IsDriver.Value);

            // Filter by HasUnhandledRequests
            if (search.HasUnhandledRequests.HasValue)
            {
                if (search.HasUnhandledRequests.Value)
                {
                    // Has at least one unhandled request
                    query = query.Where(x =>
                        x.SaveChickenRequests.Any(r => !r.IsHandled) ||
                        x.SaveChickenDriveRequests.Any(r => !r.IsHandled));
                }
                else
                {
                    // All requests are handled (or has no requests)
                    query = query.Where(x =>
                        !x.SaveChickenRequests.Any(r => !r.IsHandled) &&
                        !x.SaveChickenDriveRequests.Any(r => !r.IsHandled));
                }
            }

            // Filter by HasUnarchivedRequests
            if (search.HasUnarchivedRequests.HasValue)
            {
                if (search.HasUnarchivedRequests.Value)
                {
                    // Has at least one unarchived request
                    query = query.Where(x =>
                        x.SaveChickenRequests.Any(r => !r.IsArchived) ||
                        x.SaveChickenDriveRequests.Any(r => !r.IsArchived));
                }
                else
                {
                    // All requests are archived (or has no requests)
                    query = query.Where(x =>
                        !x.SaveChickenRequests.Any(r => !r.IsArchived) &&
                        !x.SaveChickenDriveRequests.Any(r => !r.IsArchived));
                }
            }

            // Full-text search
            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                query = query.Where(x =>
                    x.SearchVector.Matches(EF.Functions.PlainToTsQuery("german", search.SearchTerm))
                    || x.Contact.SearchVector.Matches(EF.Functions.PlainToTsQuery("german", search.SearchTerm))
                    || x.Address.SearchVector.Matches(EF.Functions.PlainToTsQuery("german", search.SearchTerm))
                );
            }

            // Sorting
            if (!string.IsNullOrEmpty(search.SortBy))
            {
                bool descending = search.SortDescending ?? false;
                string sortExpression = $"{search.SortBy} {(descending ? "descending" : "ascending")}";
                query = query.OrderBy(sortExpression);
            }
            else
            {
                // Default sorting by UpdatedAt descending
                query = query.OrderByDescending(x => x.UpdatedAt);
            }

            // Pagination
            int page = search.Page > 0 ? search.Page : 1;
            int pageSize = search.PageSize > 0 ? search.PageSize : 10;
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginationDto<Person>
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
