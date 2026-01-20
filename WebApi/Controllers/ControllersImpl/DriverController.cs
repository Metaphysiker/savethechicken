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
    public class DriverController : ControllerBase, IModelController<DriverDto, DriverSearch>
    {
        private readonly GenericModelService<Driver, DriverSearch> _service;
        private readonly AutoMapperService _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DriverController> _logger;

        public DriverController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<DriverController> logger)
        {
            _service = genericModelServiceFactory.Create<Driver, DriverSearch>();
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<DriverDto>> Create([FromBody] DriverDto dto)
        {
            var model = _mapper.mapper.Map<Driver>(dto);
            var result = await _service.Create(model, DriverIncludes.Default);
            var resultDto = _mapper.mapper.Map<DriverDto>(result);

            // Send notification email
            try
            {
                await SendNewDriverNotificationEmail(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification email for Driver {Id}", resultDto.Id);
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
        public async Task<ActionResult<DriverDto>> Read(int id)
        {
            var result = await _service.Read(id, DriverIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<DriverDto>(result);
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet]
        public async Task<ActionResult<List<DriverDto>>> ReadAll()
        {
            var result = await _service.ReadAll(DriverIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<DriverDto>>(result);
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<DriverDto>>> Search([FromBody] DriverSearch search)
        {
            var result = await _service.Search(search, DriverIncludes.Default);
            var resultDto = new PaginationDto<DriverDto>
            {
                Data = _mapper.mapper.Map<List<DriverDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut]
        public async Task<ActionResult<DriverDto>> Update([FromBody] DriverDto dto)
        {
            var model = _mapper.mapper.Map<Driver>(dto);
            var result = await _service.Update(model, DriverIncludes.Default);
            var resultDto = _mapper.mapper.Map<DriverDto>(result);
            return Ok(resultDto);
        }

        private async Task SendNewDriverNotificationEmail(DriverDto driver)
        {
            var recipients = _configuration.GetSection("Email:NotificationRecipients").Get<List<string>>();
            if (recipients == null || !recipients.Any())
            {
                _logger.LogWarning("No notification recipients configured");
                return;
            }

            var subject = $"Neuer Fahrer #{driver.Id}";
            var body = $@"
                <html>
                <body>
                    <h2>Neuer Fahrer registriert</h2>
                    <p><strong>Fahrer-ID:</strong> {driver.Id}</p>
                    <p><strong>Name:</strong> {driver.Contact?.FirstName} {driver.Contact?.LastName}</p>
                    <p><strong>E-Mail:</strong> {driver.Contact?.Email}</p>
                    <p><strong>Telefon:</strong> {driver.Contact?.PhoneNumber}</p>
                    <p><strong>Adresse:</strong> {driver.Address?.Street}, {driver.Address?.PostalCode} {driver.Address?.City}</p>
                    <p><strong>Fahrzeug:</strong> {driver.CarMake}</p>
                    <p><strong>Kapazität für Hühner:</strong> {driver.CapacityForChickens}</p>
                    <p><strong>Verfügbare Termine:</strong> {string.Join(", ", driver.AvailableDates?.Select(d => d.ToString("dd.MM.yyyy")) ?? new List<string>())}</p>
                    <p><strong>Erstellt:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                    {(driver.SaveChickenActionId.HasValue ? $"<p><strong>Zugewiesen zu Aktion:</strong> {driver.SaveChickenAction?.Title ?? driver.SaveChickenActionId.ToString()}</p>" : "")}
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(recipients, subject, body, isHtml: true);
        }
    }
}
