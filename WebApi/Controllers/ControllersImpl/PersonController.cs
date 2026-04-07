using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.DtosImpl;
using System.Linq.Expressions;
using WebApi.Database;
using WebApi.Database.Includes;
using WebApi.Factories;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services;
using WebApi.Services.ServicesImpl;

namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class PersonController : ControllerBase, IModelController<PersonDto, PersonSearch>
    {
        private readonly GenericModelService<Person, PersonSearch> _service;
        private readonly AutoMapperService _mapper;
        private readonly ILogger<PersonController> _logger;
        private readonly DatabaseContext _db;

        public PersonController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            ILogger<PersonController> logger,
            DatabaseContext db)
        {
            _service = genericModelServiceFactory.Create<Person, PersonSearch>();
            _mapper = mapper;
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<ActionResult<PersonDto>> Create([FromBody] PersonDto dto)
        {
            var model = _mapper.mapper.Map<Person>(dto);
            var result = await _service.Create(model, PersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<PersonDto>(result);
            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDto>> Read(int id)
        {
            var result = await _service.Read(id, PersonIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<PersonDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<PersonDto>>> ReadAll()
        {
            var result = await _service.ReadAll(PersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<PersonDto>>(result);
            return Ok(resultDto);
        }

        [HttpGet("autocomplete")]
        public async Task<ActionResult<List<PersonDto>>> Autocomplete([FromQuery] string searchText, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
            {
                return Ok(new List<PersonDto>());
            }

            var searchPattern = $"%{searchText}%";

            var result = await _db.Persons
                .Include(p => p.Contact)
                .Include(p => p.Address)
                .Where(p =>
                    EF.Functions.ILike(p.Contact.FirstName, searchPattern) ||
                    EF.Functions.ILike(p.Contact.LastName, searchPattern) ||
                    EF.Functions.ILike(p.Contact.Email, searchPattern))
                .OrderBy(p => p.Contact.LastName)
                .ThenBy(p => p.Contact.FirstName)
                .Take(limit)
                .ToListAsync();

            var resultDto = _mapper.mapper.Map<List<PersonDto>>(result);
            return Ok(resultDto);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<PersonDto>>> Search([FromBody] PersonSearch search)
        {
            var result = await _service.Search(search, PersonIncludes.Default);
            var resultDto = new PaginationDto<PersonDto>
            {
                Data = _mapper.mapper.Map<List<PersonDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [HttpPut]
        public async Task<ActionResult<PersonDto>> Update([FromBody] PersonDto dto)
        {
            // Load the existing person
            var existingPerson = await _db.Persons
                .Include(p => p.Contact)
                .Include(p => p.Address)
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (existingPerson == null)
            {
                return NotFound();
            }

            // Update Contact if it exists
            if (existingPerson.Contact != null && dto.Contact != null)
            {
                existingPerson.Contact.FirstName = dto.Contact.FirstName;
                existingPerson.Contact.LastName = dto.Contact.LastName;
                existingPerson.Contact.Email = dto.Contact.Email;
                existingPerson.Contact.PhoneNumber = dto.Contact.PhoneNumber;
                existingPerson.Contact.CarMake = dto.Contact.CarMake ?? string.Empty;
                existingPerson.Contact.AvailableDates = dto.Contact.AvailableDates ?? new List<DateOnly>();
                existingPerson.Contact.Categories = dto.Contact.Categories ?? new List<ContactCategory>();
                existingPerson.Contact.UpdatedAt = DateTime.UtcNow;
                _db.Contacts.Update(existingPerson.Contact);
            }

            // Update Address if it exists
            if (existingPerson.Address != null && dto.Address != null)
            {
                existingPerson.Address.Street = dto.Address.Street;
                existingPerson.Address.City = dto.Address.City;
                existingPerson.Address.PostalCode = dto.Address.PostalCode;
                existingPerson.Address.UpdatedAt = DateTime.UtcNow;
                _db.Addresses.Update(existingPerson.Address);
            }

            // Update Person properties
            existingPerson.IsBlacklisted = dto.IsBlacklisted;
            existingPerson.IsDriver = dto.IsDriver;
            existingPerson.Comment = dto.Comment;
            existingPerson.UpdatedAt = DateTime.UtcNow;
            _db.Persons.Update(existingPerson);

            await _db.SaveChangesAsync();

            // Reload with includes for the response
            var result = await _service.Read(dto.Id, PersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<PersonDto>(result);
            return Ok(resultDto);
        }

        [HttpPost("batch-update-geolocation")]
        public async Task<ActionResult<GeolocationBatchUpdateResult>> BatchUpdateGeolocation([FromBody] GeolocationBatchUpdateRequest request)
        {
            var batchSize = request.BatchSize > 0 ? request.BatchSize : 10;
            var skipCount = request.Skip > 0 ? request.Skip : 0;

            var personsQuery = _db.Persons
                .Include(p => p.Address)
                .ThenInclude(a => a!.GeoCoordinate)
                .Where(p => p.Address != null);

            // If update all is false, only update those without coordinates
            if (!request.UpdateAll)
            {
                personsQuery = personsQuery.Where(p => p.Address!.GeoCoordinate == null);
            }

            var persons = await personsQuery
                .OrderBy(p => p.Id)
                .Skip(skipCount)
                .Take(batchSize)
                .ToListAsync();

            var totalCount = await personsQuery.CountAsync();
            var result = new GeolocationBatchUpdateResult
            {
                TotalCount = totalCount,
                ProcessedCount = 0,
                UpdatedCount = 0,
                FailedCount = 0,
                FailedPersons = new List<int>()
            };

            foreach (var person in persons)
            {
                result.ProcessedCount++;

                if (person.Address == null) continue;

                try
                {
                    // Leverage existing geocoding logic from GenericModelService
                    await _service.UpdateCoordinatesAsync(person);

                    // Check if coordinates were successfully set
                    if (person.Address.GeoCoordinate != null)
                    {
                        person.Address.UpdatedAt = DateTime.UtcNow;
                        result.UpdatedCount++;
                    }
                    else
                    {
                        // Geocoding failed (no coordinates found)
                        result.FailedCount++;
                        result.FailedPersons.Add(person.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update geolocation for person {PersonId}", person.Id);
                    result.FailedCount++;
                    result.FailedPersons.Add(person.Id);
                }
            }

            if (result.UpdatedCount > 0)
            {
                await _db.SaveChangesAsync();
            }

            return Ok(result);
        }
    }

    public class GeolocationBatchUpdateRequest
    {
        public int BatchSize { get; set; } = 10;
        public int Skip { get; set; } = 0;
        public bool UpdateAll { get; set; } = false;
    }

    public class GeolocationBatchUpdateResult
    {
        public int TotalCount { get; set; }
        public int ProcessedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int FailedCount { get; set; }
        public List<int> FailedPersons { get; set; } = new List<int>();
    }
}
