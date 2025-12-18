using Microsoft.EntityFrameworkCore;
using Shared.Dtos.DtosImpl;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebApi.Models.ModelsImpl;

namespace Services.ServicesImpl
{
    public class DriverSearcher : IModelSearcher<Driver, DriverSearch>
    {
        private readonly DatabaseContext _db;
        public DriverSearcher(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<PaginationDto<Driver>> SearchAsync(DriverSearch search, params Expression<Func<Driver, object?>>[] includes)
        {
            var query = _db.Set<Driver>().AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Filter by Ids
            if (search.Ids != null && search.Ids.Any())
                query = query.Where(x => search.Ids.Contains(x.Id));

            if (search.SaveChickenActionIds != null && search.SaveChickenActionIds.Any())
                query = query.Where(x => x.SaveChickenActionId.HasValue && search.SaveChickenActionIds.Contains(x.SaveChickenActionId.Value));

            if (!string.IsNullOrEmpty(search.SearchTerm))
            {
                var term = $"%{search.SearchTerm.ToLower()}%";

                query = query.Where(x =>
                    EF.Functions.Like(x.Contact.FirstName.ToLower(), term) ||
                    EF.Functions.Like(x.Contact.LastName.ToLower(), term) ||
                    EF.Functions.Like(x.Address.Street.ToLower(), term) ||
                    EF.Functions.Like(x.Address.City.ToLower(), term) ||
                    EF.Functions.Like(x.Address.PostalCode.ToLower(), term) ||
                    EF.Functions.Like(x.Contact.PhoneNumber.ToLower(), term) ||
                    EF.Functions.Like(x.Contact.Email.ToLower(), term) ||
                    EF.Functions.Like(x.CarMake.ToLower(), term)
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

            return new PaginationDto<Driver>
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
