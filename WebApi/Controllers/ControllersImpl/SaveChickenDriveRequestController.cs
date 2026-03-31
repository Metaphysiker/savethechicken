using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
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

        public SaveChickenDriveRequestController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<SaveChickenDriveRequestController> logger)
        {
            _service = genericModelServiceFactory.Create<SaveChickenDriveRequest, SaveChickenDriveRequestSearch>();
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<SaveChickenDriveRequestDto>> Create([FromBody] SaveChickenDriveRequestDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenDriveRequest>(dto);
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
            var model = _mapper.mapper.Map<SaveChickenDriveRequest>(dto);
            var result = await _service.Update(model, SaveChickenDriveRequestIncludes.Default);
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

            var subject = $"Neuer Fahrer: {request.Person?.Contact?.FirstName} {request.Person?.Contact?.LastName}";
            var body = $@"
                <html>
                <body>
                    <h2>Neuer Fahrer</h2>
                    <p><strong>Anfrage-ID:</strong> {request.Id}</p>
                    <p><strong>Kontakt:</strong> {request.Person?.Contact?.FirstName} {request.Person?.Contact?.LastName}</p>
                    <p><strong>E-Mail:</strong> {request.Person?.Contact?.Email}</p>
                    <p><strong>Telefon:</strong> {request.Person?.Contact?.PhoneNumber}</p>
                    <p><strong>Adresse:</strong> {request.Person?.Address?.Street}, {request.Person?.Address?.PostalCode} {request.Person?.Address?.City}</p>
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
