using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.DtosImpl;
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
    public class SaveChickenDriveRequestController : ControllerBase, IModelController<SaveChickenDriveRequestDto, SaveChickenDriveRequestSearch>
    {
        private readonly GenericModelService<SaveChickenDriveRequest, SaveChickenDriveRequestSearch> _service;
        private readonly AutoMapperService _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SaveChickenDriveRequestController> _logger;
        private readonly DatabaseContext _db;

        public SaveChickenDriveRequestController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<SaveChickenDriveRequestController> logger,
            DatabaseContext db)
        {
            _service = genericModelServiceFactory.Create<SaveChickenDriveRequest, SaveChickenDriveRequestSearch>();
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<SaveChickenDriveRequestDto>> Create([FromBody] SaveChickenDriveRequestDto dto)
        {
            // Admin endpoint requires PersonId to link to existing person
            if (!dto.PersonId.HasValue || dto.PersonId.Value == 0)
            {
                return BadRequest(new { error = "PersonId is required for this endpoint. Use the public endpoint to create a request with new person data." });
            }

            var model = _mapper.mapper.Map<SaveChickenDriveRequest>(dto);

            // Admin-created requests are not from public form and are already handled
            model.IsSubmittedFromPublicForm = false;
            model.IsHandled = true;

            var result = await _service.Create(model, SaveChickenDriveRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenDriveRequestDto>(result);

            // Send notification email
            try
            {
                await SendNewDriverNotificationEmail(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification email for SaveChickenDriveRequest {Id}", resultDto.Id);
                // Don't fail the request creation if email fails
            }

            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [AllowAnonymous]
        [HttpPost("public")]
        public async Task<ActionResult<SaveChickenDriveRequestDto>> CreatePublic([FromBody] SaveChickenDriveRequestPublicDto publicDto)
        {
            // Create Contact entity
            var contact = new Contact
            {
                FirstName = publicDto.FirstName,
                LastName = publicDto.LastName,
                PhoneNumber = publicDto.PhoneNumber,
                Email = publicDto.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Contacts.Add(contact);

            // Create Address entity
            var address = new Address
            {
                Street = publicDto.Street,
                City = publicDto.City,
                PostalCode = publicDto.PostalCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Addresses.Add(address);

            // Save Contact and Address first to get their IDs
            await _db.SaveChangesAsync();

            // Create Person entity
            var person = new Person
            {
                ContactId = contact.Id,
                AddressId = address.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            // Create SaveChickenDriveRequest entity
            var request = new SaveChickenDriveRequest
            {
                PersonId = person.Id,
                CarMake = publicDto.CarMake,
                Message = publicDto.Message,
                CapacityForChickens = publicDto.CapacityForChickens,
                AvailableDates = publicDto.AvailableDates,
                SaveChickenActionId = publicDto.SaveChickenActionId,
                IsSubmittedFromPublicForm = true,
                IsHandled = false, // Public requests start as unhandled
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _service.Create(request, SaveChickenDriveRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenDriveRequestDto>(result);

            // Send notification email
            try
            {
                await SendNewDriverNotificationEmail(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification email for SaveChickenDriveRequest {Id}", resultDto.Id);
                // Don't fail the request creation if email fails
            }

            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaveChickenDriveRequestDto>> Read(int id)
        {
            var result = await _service.Read(id, SaveChickenDriveRequestIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<SaveChickenDriveRequestDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<SaveChickenDriveRequestDto>>> ReadAll()
        {
            var result = await _service.ReadAll(SaveChickenDriveRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<SaveChickenDriveRequestDto>>(result);
            return Ok(resultDto);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<SaveChickenDriveRequestDto>>> Search([FromBody] SaveChickenDriveRequestSearch search)
        {
            var result = await _service.Search(search, SaveChickenDriveRequestIncludes.Default);
            var resultDto = new PaginationDto<SaveChickenDriveRequestDto>
            {
                Data = _mapper.mapper.Map<List<SaveChickenDriveRequestDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut]
        public async Task<ActionResult<SaveChickenDriveRequestDto>> Update([FromBody] SaveChickenDriveRequestDto dto)
        {
            // Load existing entity without tracking to avoid circular reference issues
            var existing = await _db.SaveChickenDriveRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            if (existing == null) return NotFound();

            // Manually map properties to avoid including navigation properties (Person, Files)
            existing.CarMake = dto.CarMake;
            existing.CapacityForChickens = dto.CapacityForChickens;
            existing.Message = dto.Message;
            existing.AvailableDates = dto.AvailableDates;
            existing.SaveChickenActionId = dto.SaveChickenActionId;
            existing.PersonId = dto.PersonId; // Allow reassigning person (for merge)
            existing.IsHandled = dto.IsHandled; // Allow marking as handled
            existing.UpdatedAt = DateTime.UtcNow;

            // Attach and mark as modified
            _db.SaveChickenDriveRequests.Update(existing);
            await _db.SaveChangesAsync();

            // Reload with includes for the response
            var result = await _service.Read(dto.Id, SaveChickenDriveRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenDriveRequestDto>(result);
            return Ok(resultDto);
        }

        private async Task SendNewDriverNotificationEmail(SaveChickenDriveRequestDto request)
        {
            var recipients = _configuration.GetSection("Email:NotificationRecipients").Get<List<string>>();
            if (recipients == null || !recipients.Any())
            {
                _logger.LogWarning("No notification recipients configured");
                return;
            }

            // Load Person with includes
            var person = await _db.Persons
                .Include(p => p.Contact)
                .Include(p => p.Address)
                .FirstOrDefaultAsync(p => p.Id == request.PersonId);

            if (person == null)
            {
                _logger.LogWarning("Person not found for SaveChickenDriveRequest {Id}", request.Id);
                return;
            }

            var subject = $"Neuer Fahrer: {person.Contact?.FirstName} {person.Contact?.LastName}";
            var body = $@"
                <html>
                <body>
                    <h2>Neuer Fahrer</h2>
                    <p><strong>Anfrage-ID:</strong> {request.Id}</p>
                    <p><strong>Kontakt:</strong> {person.Contact?.FirstName} {person.Contact?.LastName}</p>
                    <p><strong>E-Mail:</strong> {person.Contact?.Email}</p>
                    <p><strong>Telefon:</strong> {person.Contact?.PhoneNumber}</p>
                    <p><strong>Adresse:</strong> {person.Address?.Street}, {person.Address?.PostalCode} {person.Address?.City}</p>
                    <p><strong>Auto:</strong> {request.CarMake}</p>
                    <p><strong>Kapazität:</strong> {request.CapacityForChickens} Hühner</p>
                    <p><strong>Erstellt:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                    {(request.SaveChickenActionId.HasValue ? $"<p><strong>Zugewiesen zu Aktion:</strong> {request.SaveChickenAction?.Title ?? request.SaveChickenActionId.ToString()}</p>" : "")}
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(recipients, subject, body, isHtml: true);
        }
    }
}
