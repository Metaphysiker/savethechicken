using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers.ControllersImpl;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ImportController : ControllerBase
{
    private readonly CsvImportService _importService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(CsvImportService importService, ILogger<ImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)] // 100MB limit
    public async Task<ActionResult<ImportUploadResponse>> UploadCsv([FromForm] ImportUploadRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("No file uploaded");

        if (!request.File.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only CSV files are supported");

        var allowedEntityTypes = new[] { "Person", "SaveChickenRequest", "SaveChickenDriveRequest", "Farm", "RettetDasHuhnArchive", "Fahrer", "Betrieb" };
        if (!allowedEntityTypes.Contains(request.EntityType))
            return BadRequest($"Invalid entity type. Allowed: {string.Join(", ", allowedEntityTypes)}");

        try
        {
            // Save file temporarily
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            // Start background import
            var jobId = _importService.StartImport(tempPath, request.EntityType);

            _logger.LogInformation($"Started import job {jobId} for {request.EntityType}");

            return Ok(new ImportUploadResponse
            {
                JobId = jobId,
                Status = "Processing",
                Message = $"Import started for {request.EntityType}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload CSV");
            return StatusCode(500, "Failed to upload file");
        }
    }

    [HttpGet("status/{jobId}")]
    public ActionResult<ImportStatus> GetStatus(Guid jobId)
    {
        var status = _importService.GetStatus(jobId);
        
        if (status == null)
            return NotFound($"Import job {jobId} not found");

        return Ok(status);
    }
}

public class ImportUploadRequest
{
    public required IFormFile File { get; set; }
    public required string EntityType { get; set; }
}

public class ImportUploadResponse
{
    public Guid JobId { get; set; }
    public required string Status { get; set; }
    public required string Message { get; set; }
}
