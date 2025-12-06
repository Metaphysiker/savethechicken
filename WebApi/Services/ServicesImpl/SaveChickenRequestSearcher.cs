using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.DtosImpl;
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

        public async Task<PaginationDto<SaveChickenRequest>> SearchAsync(SaveChickenRequestSearch search)
        {
            var query = _db.Set<SaveChickenRequest>().AsQueryable();

            // Filter by Ids
            if (search.Ids != null && search.Ids.Any())
                query = query.Where(x => search.Ids.Contains(x.Id));

            // Filter by SaveChickenActionIds
            if (search.SaveChickenActionIds != null && search.SaveChickenActionIds.Any())
                query = query.Where(x => x.SaveChickenActionId.HasValue && search.SaveChickenActionIds.Contains(x.SaveChickenActionId.Value));

            // Filter by SearchTerm (OR logic)
            if (!string.IsNullOrEmpty(search.SearchTerm))
            {
                    var term = $"%{search.SearchTerm.ToLower()}%";


                query = query.Where(x =>
                    EF.Functions.Like(x.FirstName.ToLower(), term) ||
                    EF.Functions.Like(x.LastName.ToLower(), term) ||
                    EF.Functions.Like(x.Street.ToLower(), term) ||
                    EF.Functions.Like(x.City.ToLower(), term) ||
                    EF.Functions.Like(x.PostalCode.ToLower(), term) ||
                    EF.Functions.Like(x.PhoneNumber.ToLower(), term) ||
                    EF.Functions.Like(x.Email.ToLower(), term) ||
                    EF.Functions.Like(x.DescriptionOfPlaceForChickens.ToLower(), term) ||
                    EF.Functions.Like(x.Message.ToLower(), term)
                );
            }

            // Sorting
            if (!string.IsNullOrEmpty(search.SortBy))
            {
                bool descending = search.SortDescending ?? false;
                query = descending
                    ? query.OrderByDescending(e => EF.Property<object>(e, search.SortBy))
                    : query.OrderBy(e => EF.Property<object>(e, search.SortBy));
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
