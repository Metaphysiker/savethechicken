using Microsoft.EntityFrameworkCore;
using Shared.Dtos.DtosImpl;
using System.Linq.Expressions;
using WebApi.Interfaces;
using WebApi.Models.ModelsImpl;

namespace WebApi.Services.ServicesImpl
{
    public class GenericModelService<TModel, TSearchDto> : IModelService<TModel, TSearchDto>
        where TModel : class, IModel
        where TSearchDto : ISearchDto
    {
        private ModelSearchFactory _modelSearchFactory;
        private GeoLocatorService _geoLocatorService;

        private readonly DatabaseContext _db;
        public GenericModelService(DatabaseContext db, ModelSearchFactory modelSearchFactory)
        {
            _db = db;
            _modelSearchFactory = modelSearchFactory;
            _geoLocatorService = new GeoLocatorService();
        }

        public async Task UpdateCoordinatesAsync(TModel model)
        {
            if (model is IEntityWithAddress entityWithAddress)
            {
                if (entityWithAddress.Address != null)
                {
                    var geoCoordinates = await _geoLocatorService.GetCoordinatesAsync(entityWithAddress.Address.Street, entityWithAddress.Address.PostalCode, entityWithAddress.Address.City);
                    if (geoCoordinates != null)
                    {
                        entityWithAddress.Address.GeoCoordinate = geoCoordinates;
                    }
                }
            }
        }

        public async Task UpdateIsActiveInSaveChickenActions(TModel model)
        {
            {
                if (model is SaveChickenAction saveChickenAction && saveChickenAction.IsActive)
                {
                    var others = _db.Set<SaveChickenAction>()
                        .Where(a => a.Id != saveChickenAction.Id && a.IsActive);

                    await others.ForEachAsync(a => a.IsActive = false);
                }
            }
        }

        public async Task UpdateFiles(TModel model)
        {
            if (model is not IEntityWithFiles incoming)
                return;

            var parentType = typeof(TModel);
            var fkPropertyName = parentType.Name + "Id";

            // 1. Fetch existing files WITHOUT tracking them
            var existingFiles = await _db.Set<StoredFile>()
                .AsNoTracking() // This prevents the ID conflict
                .Where(f => EF.Property<int?>(f, fkPropertyName) == incoming.Id)
                .ToListAsync();

            // 2. Identify which files need to be deleted
            var incomingIds = incoming.Files.Where(f => f.Id > 0).Select(f => f.Id).ToHashSet();
            var toRemove = existingFiles
                .Where(f => !incomingIds.Contains(f.Id))
                .Select(f => new StoredFile { Id = f.Id }) // Create "stub" for deletion
                .ToList();

            if (toRemove.Any())
            {
                // We must attach and remove because these weren't being tracked
                _db.Set<StoredFile>().RemoveRange(toRemove);
            }

            // 3. Handle Foreign Keys for new files
            foreach (var file in incoming.Files)
            {
                if (file.Id == 0)
                {
                    // Ensure the Shadow Property/FK is set so EF knows where it belongs
                    _db.Entry(file).Property(fkPropertyName).CurrentValue = incoming.Id;
                }
                else
                {
                    // Note: We don't need to manually update existing files here.
                    // Because you are calling _db.Update(model), EF will automatically
                    // detect changes in the Files collection of the model and 
                    // generate UPDATE statements for them.
                }
            }
        }

        public async Task<TModel> Create(TModel model, params Expression<Func<TModel, object?>>[] includes)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            await UpdateCoordinatesAsync(model);
            await UpdateIsActiveInSaveChickenActions(model);
            await UpdateFiles(model);

            _db.Set<TModel>().Add(model);
            await _db.SaveChangesAsync();
            return await Read(model.Id, includes) ?? model;
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

        public async Task<TModel?> Read(int id, params Expression<Func<TModel, object?>>[] includes)
        {
            if (includes.Length > 0)
            {
                var query = _db.Set<TModel>().AsQueryable();

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
            }

            return await _db.Set<TModel>().FindAsync(id);
        }

        public async Task<List<TModel>> ReadAll(params Expression<Func<TModel, object?>>[] includes)
        {
            if (includes.Length > 0)
            {
                var query = _db.Set<TModel>().AsQueryable();

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                return await query.ToListAsync();
            }

            return await _db.Set<TModel>().ToListAsync();
        }

        public async Task<PaginationDto<TModel>> Search(TSearchDto search, params Expression<Func<TModel, object?>>[] includes)
        {
            var searcher = _modelSearchFactory.Create<TModel, TSearchDto>();
            return await searcher.SearchAsync(search, includes);
        }

        public async Task<TModel> Update(TModel model, params Expression<Func<TModel, object?>>[] includes)
        {
            model.UpdatedAt = DateTime.UtcNow;

            await UpdateCoordinatesAsync(model);
            await UpdateIsActiveInSaveChickenActions(model);
            await UpdateFiles(model);

            _db.Set<TModel>().Update(model);
            await _db.SaveChangesAsync();

            if (includes.Length > 0)
            {
                return await Read(model.Id, includes) ?? model;
            }

            return model;
        }
    }
}
