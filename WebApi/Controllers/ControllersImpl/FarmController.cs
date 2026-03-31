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
    public class FarmController : ControllerBase, IModelController<FarmDto, FarmSearch>
    {
        private readonly GenericModelService<Farm, FarmSearch> _service;
        private readonly AutoMapperService _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FarmController> _logger;

        public FarmController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<FarmController> logger)
        {
            _service = genericModelServiceFactory.Create<Farm, FarmSearch>();
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<FarmDto>> Create([FromBody] FarmDto dto)
        {
            var model = _mapper.mapper.Map<Farm>(dto);
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
                    <p><strong>Verfügbare Termine:</strong> {string.Join(", ", farm.DatesForRescues?.Select(d => d.ToString("dd.MM.yyyy")) ?? new List<string>())}</p>
                    <p><strong>Erstellt:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                    {(farm.SaveChickenActionId.HasValue ? $"<p><strong>Zugewiesen zu Aktion:</strong> {farm.SaveChickenAction?.Title ?? farm.SaveChickenActionId.ToString()}</p>" : "")}
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(recipients, subject, body, isHtml: true);
        }

    }
}
