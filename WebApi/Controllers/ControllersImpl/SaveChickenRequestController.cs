using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using System.Linq.Expressions;
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

        public SaveChickenRequestController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<SaveChickenRequestController> logger)
        {
            _service = genericModelServiceFactory.Create<SaveChickenRequest, SaveChickenRequestSearch>();
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost]
        public async Task<ActionResult<SaveChickenRequestDto>> Create([FromBody] SaveChickenRequestDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenRequest>(dto);
            var result = await _service.Create(model, SaveChickenRequestIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenRequestDto>(result);

            // Send notification email
            try
            {
                await SendNewRequestNotificationEmail(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification email for SaveChickenRequest {Id}", resultDto.Id);
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
            var model = _mapper.mapper.Map<SaveChickenRequest>(dto);
            var result = await _service.Update(model, SaveChickenRequestIncludes.Default);
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

            var subject = $"Neuer Abnehmer: {request.Contact?.FirstName} {request.Contact?.LastName}";
            var body = $@"
                <html>
                <body>
                    <h2>Neuer Abnehmer</h2>
                    <p><strong>Anfrage-ID:</strong> {request.Id}</p>
                    <p><strong>Kontakt:</strong> {request.Contact?.FirstName} {request.Contact?.LastName}</p>
                    <p><strong>E-Mail:</strong> {request.Contact?.Email}</p>
                    <p><strong>Telefon:</strong> {request.Contact?.PhoneNumber}</p>
                    <p><strong>Adresse:</strong> {request.Address?.Street}, {request.Address?.PostalCode} {request.Address?.City}</p>
                    <p><strong>Anzahl Hühner:</strong> {request.NumberOfChickensToBeSaved}</p>
                    <p><strong>Anzahl Hähne:</strong> {request.NumberOfRoostersToBeSaved}</p>
                    <p><strong>Erstellt:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                    {(request.SaveChickenActionId.HasValue ? $"<p><strong>Zugewiesen zu Aktion:</strong> {request.SaveChickenAction?.Title ?? request.SaveChickenActionId.ToString()}</p>" : "")}
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(recipients, subject, body, isHtml: true);
        }

    }
}
