using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class FarmController : ControllerBase, IModelController<FarmDto, FarmSearch>
    {
        private readonly GenericModelService<Farm, FarmSearch> _service;
        private readonly AutoMapperService _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FarmController> _logger;
        private readonly DatabaseContext _db;

        public FarmController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<FarmController> logger,
            DatabaseContext db)
        {
            _service = genericModelServiceFactory.Create<Farm, FarmSearch>();
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<FarmDto>> Create([FromBody] FarmDto dto)
        {
            // Clear validation errors for nested Contact/Address if we're creating new ones
            if (dto.ContactId == 0 && dto.Contact != null)
            {
                // Remove validation errors for Contact properties
                var contactErrors = ModelState.Keys
                    .Where(k => k.StartsWith("Contact."))
                    .ToList();
                foreach (var key in contactErrors)
                {
                    ModelState.Remove(key);
                }
            }

            if (dto.AddressId == 0 && dto.Address != null)
            {
                // Remove validation errors for Address properties
                var addressErrors = ModelState.Keys
                    .Where(k => k.StartsWith("Address."))
                    .ToList();
                foreach (var key in addressErrors)
                {
                    ModelState.Remove(key);
                }
            }

            // Now check if model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // If ContactId and AddressId are not provided (or are 0), create new entities from the nested objects
            if (dto.ContactId == 0 && dto.Contact != null)
            {
                // Validate contact fields manually
                if (string.IsNullOrWhiteSpace(dto.Contact.FirstName) ||
                    string.IsNullOrWhiteSpace(dto.Contact.LastName) ||
                    string.IsNullOrWhiteSpace(dto.Contact.Email) ||
                    string.IsNullOrWhiteSpace(dto.Contact.PhoneNumber))
                {
                    return BadRequest(new { error = "Contact information is incomplete." });
                }

                var contact = _mapper.mapper.Map<Contact>(dto.Contact);
                contact.CreatedAt = DateTime.UtcNow;
                contact.UpdatedAt = DateTime.UtcNow;
                _db.Contacts.Add(contact);
                await _db.SaveChangesAsync();
                dto.ContactId = contact.Id;
            }

            if (dto.AddressId == 0 && dto.Address != null)
            {
                // Validate address fields manually
                if (string.IsNullOrWhiteSpace(dto.Address.Street) ||
                    string.IsNullOrWhiteSpace(dto.Address.City) ||
                    string.IsNullOrWhiteSpace(dto.Address.PostalCode))
                {
                    return BadRequest(new { error = "Address information is incomplete." });
                }

                var address = _mapper.mapper.Map<Address>(dto.Address);
                address.CreatedAt = DateTime.UtcNow;
                address.UpdatedAt = DateTime.UtcNow;
                _db.Addresses.Add(address);
                await _db.SaveChangesAsync();
                dto.AddressId = address.Id;
            }

            // Now ContactId and AddressId should be set
            if (dto.ContactId == 0 || dto.AddressId == 0)
            {
                return BadRequest(new { error = "Contact and Address information is required." });
            }

            var model = _mapper.mapper.Map<Farm>(dto);

            // Admin-created farms are not from public form and are already handled
            model.IsSubmittedFromPublicForm = false;
            model.IsHandled = true;

            var result = await _service.Create(model, FarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<FarmDto>(result);

            // Send notification email
            try
            {
                await SendNewFarmNotificationEmail(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification email for Farm {Id}", resultDto.Id);
                // Don't fail the request creation if email fails
            }

            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [AllowAnonymous]
        [HttpPost("public")]
        public async Task<ActionResult<FarmDto>> CreatePublic([FromBody] FarmPublicDto publicDto)
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

            // Create Farm entity
            var farm = new Farm
            {
                Name = publicDto.Name,
                ContactId = contact.Id,
                AddressId = address.Id,
                NumberOfChickens = publicDto.NumberOfChickens,
                NumberOfRoosters = publicDto.NumberOfRoosters,
                Size = publicDto.Size,
                Color = publicDto.Color,
                GeneralInformation = publicDto.GeneralInformation,
                SaveChickenActionId = publicDto.SaveChickenActionId,
                IsSubmittedFromPublicForm = true,
                IsHandled = false, // Public farms start as unhandled
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _service.Create(farm, FarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<FarmDto>(result);

            // Send notification email
            try
            {
                await SendNewFarmNotificationEmail(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification email for Farm {Id}", resultDto.Id);
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

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet("{id}")]
        public async Task<ActionResult<FarmDto>> Read(int id)
        {
            var result = await _service.Read(id, FarmIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<FarmDto>(result);
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet]
        public async Task<ActionResult<List<FarmDto>>> ReadAll()
        {
            var result = await _service.ReadAll(FarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<FarmDto>>(result);
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<FarmDto>>> Search([FromBody] FarmSearch search)
        {
            var result = await _service.Search(search, FarmIncludes.Default);
            var resultDto = new PaginationDto<FarmDto>
            {
                Data = _mapper.mapper.Map<List<FarmDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut]
        public async Task<ActionResult<FarmDto>> Update([FromBody] FarmDto dto)
        {
            var model = _mapper.mapper.Map<Farm>(dto);
            var result = await _service.Update(model, FarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<FarmDto>(result);
            return Ok(resultDto);
        }

        private async Task SendNewFarmNotificationEmail(FarmDto farm)
        {
            var recipients = _configuration.GetSection("Email:NotificationRecipients").Get<List<string>>();
            if (recipients == null || !recipients.Any())
            {
                _logger.LogWarning("No notification recipients configured");
                return;
            }

            var subject = $"Neuer Betrieb #{farm.Id}";
            var body = $@"
                <html>
                <body>
                    <h2>Neuer Betrieb registriert</h2>
                    <p><strong>Betriebs-ID:</strong> {farm.Id}</p>
                    <p><strong>Name:</strong> {farm.Contact?.FirstName} {farm.Contact?.LastName}</p>
                    <p><strong>E-Mail:</strong> {farm.Contact?.Email}</p>
                    <p><strong>Telefon:</strong> {farm.Contact?.PhoneNumber}</p>
                    <p><strong>Adresse:</strong> {farm.Address?.Street}, {farm.Address?.PostalCode} {farm.Address?.City}</p>
                    <p><strong>Anzahl Hühner:</strong> {farm.NumberOfChickens}</p>
                    <p><strong>Anzahl Hähne:</strong> {farm.NumberOfRoosters}</p>
                    <p><strong>Erstellt:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                    {(farm.SaveChickenActionId.HasValue ? $"<p><strong>Zugewiesen zu Aktion:</strong> {farm.SaveChickenAction?.Title ?? farm.SaveChickenActionId.ToString()}</p>" : "")}
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(recipients, subject, body, isHtml: true);
        }

    }
}
