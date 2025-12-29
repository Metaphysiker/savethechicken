using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Shared.CsvRecords;
using Shared.Dtos.DtosImpl;
using System.Globalization;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;

namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsvController : ControllerBase
    {

        private readonly GenericModelService<SaveChickenAction, SaveChickenActionSearch> _saveChickenActionService;
        private readonly GenericModelService<SaveChickenRequest, SaveChickenRequestSearch> _saveChickenRequestService;

        public CsvController(GenericModelServiceFactory genericModelServiceFactory)
        {
            _saveChickenActionService = genericModelServiceFactory.Create<SaveChickenAction, SaveChickenActionSearch>();
            _saveChickenRequestService = genericModelServiceFactory.Create<SaveChickenRequest, SaveChickenRequestSearch>();
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCsvForArchive(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("CSV file is required.");

            SaveChickenAction saveChickenAction = new SaveChickenAction
            {
                Title = "Archiv (vor 2026)",
            };

            var archive = await _saveChickenActionService.Create(saveChickenAction);

            if (archive == null)
                return StatusCode(500, "Failed to create archive action.");

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var saveChickenRequests = new List<SaveChickenRequest>();

            var records = csv.GetRecords<RettetDasHuhnCsvRecord>();

            foreach (var record in records)
            {
                var saveChickenRequest = new SaveChickenRequest();
                saveChickenRequest.Contact = new Contact
                {
                    FirstName = record.Vorname,
                    LastName = record.Name,
                    Email = record.Email,
                    PhoneNumber = record.Telefon1 ?? record.Telefon2
                };
                saveChickenRequest.Address = new Address
                {
                    Street = record.Strasse,
                    City = record.Ort,
                    PostalCode = record.PLZ
                };
                saveChickenRequests.Add(saveChickenRequest);
            }

            foreach (var request in saveChickenRequests)
            {
                request.SaveChickenActionId = archive.Id;
                await _saveChickenRequestService.Create(request);
            }

            return Ok("CSV processed successfully.");
        }
    }
}
