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
    public class SaveChickenRequestController : ControllerBase, IModelController<SaveChickenRequestDto, SaveChickenRequestSearch>
    {
        private readonly GenericModelService<SaveChickenRequest, SaveChickenRequestSearch> _service;
        private readonly AutoMapperService _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SaveChickenRequestController> _logger;
        private readonly DatabaseContext _db;

        public SaveChickenRequestController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<SaveChickenRequestController> logger,
            DatabaseContext db)
        {
            _service = genericModelServiceFactory.Create<SaveChickenRequest, SaveChickenRequestSearch>();
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<SaveChickenRequestDto>> Create([FromBody] SaveChickenRequestDto dto)
        {
            // Admin endpoint requires PersonId to link to existing person
            if (!dto.PersonId.HasValue || dto.PersonId.Value == 0)
            {
                return BadRequest(new { error = "PersonId is required for this endpoint. Use the public endpoint to create a request with new person data." });
            }

            var model = _mapper.mapper.Map<SaveChickenRequest>(dto);

            // Admin-created requests are not from public form and are already handled
            model.IsSubmittedFromPublicForm = false;
            model.IsHandled = true;

            var result = await _service.Create(model, SaveChickenRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenRequestDto>(result);

            // Send notification email to admins
            try
            {
                await SendNewRequestNotificationEmail(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification email for SaveChickenRequest {Id}", resultDto.Id);
                // Don't fail the request creation if email fails
            }

            // Send confirmation email to requester
            try
            {
                await SendConfirmationEmailToRequester(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to requester for SaveChickenRequest {Id}", resultDto.Id);
                // Don't fail the request creation if email fails
            }

            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [AllowAnonymous]
        [HttpPost("public")]
        public async Task<ActionResult<SaveChickenRequestDto>> CreatePublic([FromBody] SaveChickenRequestPublicDto publicDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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

            // Create main Address entity
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

            // Create SaveChickenRequest entity
            var request = new SaveChickenRequest
            {
                PersonId = person.Id,
                NumberOfChickensToBeSaved = publicDto.NumberOfChickensToBeSaved,
                NumberOfRoostersToBeSaved = publicDto.NumberOfRoostersToBeSaved,
                DescriptionOfPlaceForChickens = publicDto.DescriptionOfPlaceForChickens,
                AcceptTermsAndConditions = publicDto.AcceptTermsAndConditions,
                ConfirmThatIFulfillCriteria = publicDto.ConfirmThatIFulfillCriteria,
                Message = publicDto.Message,
                SaveChickenActionId = publicDto.SaveChickenActionId,
                NumberOfBoxes = publicDto.NumberOfBoxes,
                Color = publicDto.Color,
                IsSubmittedFromPublicForm = true,
                IsHandled = false, // Public requests start as unhandled
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add files if present
            if (publicDto.Files != null && publicDto.Files.Count > 0)
            {
                foreach (var fileDto in publicDto.Files)
                {
                    var storedFile = new StoredFile
                    {
                        FileName = fileDto.FileName,
                        ContentType = fileDto.ContentType,
                        FileKey = fileDto.FileKey,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    request.Files.Add(storedFile);
                }
            }

            var result = await _service.Create(request, SaveChickenRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenRequestDto>(result);

            // Send confirmation email to requester
            try
            {
                await SendConfirmationEmailToRequester(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to requester for SaveChickenRequest {Id}", resultDto.Id);
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
        public async Task<ActionResult<SaveChickenRequestDto>> Read(int id)
        {
            var result = await _service.Read(id, SaveChickenRequestIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<SaveChickenRequestDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<SaveChickenRequestDto>>> ReadAll()
        {
            var result = await _service.ReadAll(SaveChickenRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<SaveChickenRequestDto>>(result);
            return Ok(resultDto);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<SaveChickenRequestDto>>> Search([FromBody] SaveChickenRequestSearch search)
        {
            var result = await _service.Search(search, SaveChickenRequestIncludes.Default);
            var resultDto = new PaginationDto<SaveChickenRequestDto>
            {
                Data = _mapper.mapper.Map<List<SaveChickenRequestDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut]
        public async Task<ActionResult<SaveChickenRequestDto>> Update([FromBody] SaveChickenRequestDto dto)
        {
            // Load the existing entity (basic properties only)
            var existing = await _db.SaveChickenRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            if (existing == null)
            {
                return NotFound();
            }

            // Map only the properties we want to update
            existing.NumberOfChickensToBeSaved = dto.NumberOfChickensToBeSaved;
            existing.NumberOfRoostersToBeSaved = dto.NumberOfRoostersToBeSaved;
            existing.NumberOfBoxes = dto.NumberOfBoxes;
            existing.DescriptionOfPlaceForChickens = dto.DescriptionOfPlaceForChickens;
            existing.AcceptTermsAndConditions = dto.AcceptTermsAndConditions;
            existing.ConfirmThatIFulfillCriteria = dto.ConfirmThatIFulfillCriteria;
            existing.Message = dto.Message;
            existing.SaveChickenActionId = dto.SaveChickenActionId;
            existing.Color = dto.Color;
            existing.PersonId = dto.PersonId; // Allow reassigning person (for merge)
            existing.IsHandled = dto.IsHandled; // Allow marking as handled
            existing.UpdatedAt = DateTime.UtcNow;

            // Attach and mark as modified
            _db.SaveChickenRequests.Update(existing);
            await _db.SaveChangesAsync();

            // Reload with includes for the response
            var result = await _service.Read(dto.Id, SaveChickenRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenRequestDto>(result);
            return Ok(resultDto);
        }

        private async Task SendNewRequestNotificationEmail(SaveChickenRequestDto request)
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
                _logger.LogWarning("Person not found for SaveChickenRequest {Id}", request.Id);
                return;
            }

            var subject = $"Neuer Abnehmer: {person.Contact?.FirstName} {person.Contact?.LastName}";
            var body = $@"
                <html>
                <body>
                    <h2>Neuer Abnehmer</h2>
                    <p><strong>Kontakt:</strong> {person.Contact?.FirstName} {person.Contact?.LastName}</p>
                    <p><strong>E-Mail:</strong> {person.Contact?.Email}</p>
                    <p><strong>Telefon:</strong> {person.Contact?.PhoneNumber}</p>
                    <p><strong>Adresse:</strong> {person.Address?.Street}, {person.Address?.PostalCode} {person.Address?.City}</p>
                    <p><strong>Anzahl Hühner:</strong> {request.NumberOfChickensToBeSaved}</p>
                    <p><strong>Anzahl Hähne:</strong> {request.NumberOfRoostersToBeSaved}</p>
                    <p><strong>Erstellt:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(recipients, subject, body, isHtml: true);
        }

        private async Task SendConfirmationEmailToRequester(SaveChickenRequestDto request)
        {
            // Load Person with includes
            var person = await _db.Persons
                .Include(p => p.Contact)
                .Include(p => p.Address)
                .FirstOrDefaultAsync(p => p.Id == request.PersonId);

            if (person == null)
            {
                _logger.LogWarning("Person not found for SaveChickenRequest {Id}", request.Id);
                return;
            }

            var email = person.Contact?.Email;
            Console.WriteLine($"Attempting to send confirmation email to requester at {email} for SaveChickenRequest {request.Id}");
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No email address for requester in SaveChickenRequest {Id}", request.Id);
                return;
            }

            var subject = "Vielen Dank für Ihre Anfrage - Rettet das Huhn";
            var body = $@"
                <html>
                <body>
                    <h2>Vielen Dank für Ihre Anfrage!</h2>
                    <p>Liebe/r {person.Contact?.FirstName} {person.Contact?.LastName},</p>

                    <p>Vielen Dank, dass Sie Hühnern in Not ein neues Zuhause geben möchten!</p>

                    <p>Wir haben Ihre Anfrage erhalten und werden uns in Kürze bei Ihnen melden.</p>

                    <h3>Ihre Angaben:</h3>
                    <ul>
                        <li><strong>Anfrage-Nr.:</strong> {request.Id}</li>
                        <li><strong>Anzahl Hühner:</strong> {request.NumberOfChickensToBeSaved}</li>
                        <li><strong>Anzahl Hähne:</strong> {request.NumberOfRoostersToBeSaved}</li>
                        <li><strong>Ort:</strong> {person.Address?.PostalCode} {person.Address?.City}</li>
                        <li><strong>Strasse:</strong> {person.Address?.Street}</li>
                        <li><strong>E-Mail:</strong> {person.Contact?.Email}</li>
                        <li><strong>Telefon:</strong> {person.Contact?.PhoneNumber}</li>
                        <li><strong>Beschreibung des Ortes:</strong> {request.DescriptionOfPlaceForChickens}</li>
                        <li><strong>Ich bestätige, dass ich die Voraussetzungen erfülle und die Hühnerhaltung ohne Einschränkung in mein Leben passt:</strong> {(request.ConfirmThatIFulfillCriteria ? "Ja" : "Nein")}</li>
                    </ul>

                    <p>Wir werden Sie kontaktieren, sobald wir Hühner aus der nächsten Rettung für Sie haben.</p>

                    <p>Mit freundlichen Grüßen<br/>
                    Ihr Team von Rettet das Huhn</p>
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(new List<string> { email }, subject, body, isHtml: true);
        }

    }
}
