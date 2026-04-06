using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Shared.Dtos.DtosImpl;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebApi.Database.Includes;
using WebApi.Models.ModelsImpl;

namespace Services.ServicesImpl
{
    public class SaveChickenRequestSearcher : IModelSearcher<SaveChickenRequest, SaveChickenRequestSearch>
    {
        private readonly DatabaseContext _db;
        public SaveChickenRequestSearcher(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<PaginationDto<SaveChickenRequest>> SearchAsync(SaveChickenRequestSearch search, params Expression<Func<SaveChickenRequest, object?>>[] includes)
        {
            var query = _db.Set<SaveChickenRequest>().AsQueryable();
            
            // Use string-based includes for proper navigation property chaining
            // String-based Include properly loads nested entities like Person.Contact
            foreach (var includePath in SaveChickenRequestIncludes.DefaultStrings)
            {
                query = query.Include(includePath);
            }

            // Filter by Ids
            if (search.Ids != null && search.Ids.Any())
                query = query.Where(x => search.Ids.Contains(x.Id));

            // Filter by SaveChickenActionIds
            if (search.SaveChickenActionIds != null && search.SaveChickenActionIds.Any())
                query = query.Where(x => x.SaveChickenActionId.HasValue && search.SaveChickenActionIds.Contains(x.SaveChickenActionId.Value));

            // Filter by PersonId
            if (search.PersonId.HasValue)
                query = query.Where(x => x.PersonId == search.PersonId.Value);

            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                query = query.Where(x =>
                    x.SearchVector.Matches(EF.Functions.PlainToTsQuery("german", search.SearchTerm))
                    || x.Person.Contact.SearchVector.Matches(EF.Functions.PlainToTsQuery("german", search.SearchTerm))
                    || x.Person.Address.SearchVector.Matches(EF.Functions.PlainToTsQuery("german", search.SearchTerm))
                );
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

            return new PaginationDto<SaveChickenRequest>
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
