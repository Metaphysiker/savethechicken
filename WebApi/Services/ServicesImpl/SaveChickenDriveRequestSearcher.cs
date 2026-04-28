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
    public class SaveChickenDriveRequestSearcher : IModelSearcher<SaveChickenDriveRequest, SaveChickenDriveRequestSearch>
    {
        private readonly DatabaseContext _db;
        public SaveChickenDriveRequestSearcher(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<PaginationDto<SaveChickenDriveRequest>> SearchAsync(SaveChickenDriveRequestSearch search, params Expression<Func<SaveChickenDriveRequest, object?>>[] includes)
        {
            var query = _db.Set<SaveChickenDriveRequest>().AsQueryable();

            // Use string-based includes for proper navigation property chaining
            // String-based Include properly loads nested entities like Person.Contact
            foreach (var includePath in SaveChickenDriveRequestIncludes.DefaultStrings)
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

            // Filter by IsHandled
            if (search.IsHandled.HasValue)
                query = query.Where(x => x.IsHandled == search.IsHandled.Value);

            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                // Search in drive request fields and related Person data
                query = query.Where(x =>
                    EF.Functions.ILike(x.CarMake, $"%{search.SearchTerm}%")
                    || EF.Functions.ILike(x.Person.Contact.FirstName, $"%{search.SearchTerm}%")
                    || EF.Functions.ILike(x.Person.Contact.LastName, $"%{search.SearchTerm}%")
                    || EF.Functions.ILike(x.Person.Contact.Email, $"%{search.SearchTerm}%")
                    || EF.Functions.ILike(x.Person.Address.City, $"%{search.SearchTerm}%")
                    || EF.Functions.ILike(x.Person.Address.Street, $"%{search.SearchTerm}%")
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

            return new PaginationDto<SaveChickenDriveRequest>
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
