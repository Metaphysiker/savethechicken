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
        private readonly GenericModelService<Driver, DriverSearch> _driverService;
        private readonly GenericModelService<Farm, FarmSearch> _farmService;

        public CsvController(GenericModelServiceFactory genericModelServiceFactory)
        {
            _saveChickenActionService = genericModelServiceFactory.Create<SaveChickenAction, SaveChickenActionSearch>();
            _saveChickenRequestService = genericModelServiceFactory.Create<SaveChickenRequest, SaveChickenRequestSearch>();
            _driverService = genericModelServiceFactory.Create<Driver, DriverSearch>();
            _farmService = genericModelServiceFactory.Create<Farm, FarmSearch>();
        }


        [HttpPost("upload/save-chicken-request")]
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
                saveChickenRequest.Message = record.Bemerkungen + " " + record.Zusatz;
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

        [HttpPost("upload/farm")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCsvForFarm(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("CSV file is required.");

            
            var search = new SaveChickenActionSearch
            {
                Title = "Archiv (vor 2026)"
            };

            var results = await _saveChickenActionService.Search(search);

            if(results.Data.Count == 0)
            {
                return BadRequest("Archive action does not exist");
            }

            SaveChickenAction archive = results.Data.First();


            if (archive == null)
                return StatusCode(500, "Failed to create archive action.");

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var farms = new List<Farm>();

            var records = csv.GetRecords<BetriebCsvRecord>();

            foreach (var record in records)
            {

                var farm = new Farm();
                farm.GeneralInformation = record.Bemerkungen + " " + record.Zusatz;
                farm.Color = record.Farbe;
                farm.Name = record.Vorname + " " + record.Name + ", " + record.Ort;
                farm.Address = new Address
                {
                    Street = record.Strasse,
                    City = record.Ort,
                    PostalCode = record.PLZ
                };

                farm.Contact = new Contact
                {
                    FirstName = record.Vorname,
                    LastName = record.Name,
                    Email = "",
                    PhoneNumber = record.Telefon1 ?? record.Telefonf2
                };

                farms.Add(farm);
            }

            foreach (var request in farms)
            {
                request.SaveChickenActionId = archive.Id;
                await _farmService.Create(request);
            }

            return Ok("CSV processed successfully.");
        }

        [HttpPost("upload/driver")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCsvForDriver(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("CSV file is required.");

            var search = new SaveChickenActionSearch
            {
                Title = "Archiv (vor 2026)"
            };

            var results = await _saveChickenActionService.Search(search);

            if (results.Data.Count == 0)
            {
                return BadRequest("Archive action does not exist");
            }

            SaveChickenAction archive = results.Data.First();

            if (archive == null)
                return StatusCode(500, "Failed to create archive action.");

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var drivers = new List<Driver>();

            var records = csv.GetRecords<FahrerCsvRecord>();

            foreach (var record in records)
            {
                var driver = new Driver();

                driver.CarMake = record.Fahrzeug;

                driver.Contact = new Contact
                {
                    FirstName = record.Vorname,
                    LastName = record.Name,
                    Email = record.Email,
                    PhoneNumber = record.Telefon1 ?? record.Telefon2
                };

                driver.Address = new Address
                {
                    Street = record.Strasse,
                    City = record.Ort,
                    PostalCode = record.PLZ
                };

                drivers.Add(driver);
            }

            foreach (var request in drivers)
            {
                request.SaveChickenActionId = archive.Id;
                await _driverService.Create(request);
            }

            return Ok("CSV processed successfully.");
        }
    }
}
