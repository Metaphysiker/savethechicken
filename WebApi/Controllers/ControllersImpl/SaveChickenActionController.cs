using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using WebApi.Database.Includes;
using WebApi.Factories;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;
using System.IO.Compression;
using QuestPDF.Fluent;
using Shared.Classes;
using System.Text.Json;
namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaveChickenActionController : ControllerBase, IModelController<SaveChickenActionDto, SaveChickenActionSearch>
    {
        private readonly GenericModelService<SaveChickenAction, SaveChickenActionSearch> _service;
        private readonly GenericModelService<SaveChickenRequest, SaveChickenRequestSearch> _saveChickenRequestService;

        private readonly AutoMapperService _mapper;

        public SaveChickenActionController(GenericModelServiceFactory genericModelServiceFactory, AutoMapperService mapper)
        {
            _service = genericModelServiceFactory.Create<SaveChickenAction, SaveChickenActionSearch>();
            _saveChickenRequestService = genericModelServiceFactory.Create<SaveChickenRequest, SaveChickenRequestSearch>();
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<SaveChickenActionDto>> Create([FromBody] SaveChickenActionDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenAction>(dto);
            var result = await _service.Create(model);
            var resultDto = _mapper.mapper.Map<SaveChickenActionDto>(result);
            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<SaveChickenActionDto>> Read(int id)
        {
            var result = await _service.Read(id);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<SaveChickenActionDto>(result);
            return Ok(resultDto);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<SaveChickenActionDto>>> ReadAll()
        {
            var result = await _service.ReadAll();
            var resultDto = _mapper.mapper.Map<List<SaveChickenActionDto>>(result);
            return Ok(resultDto);
        }

        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<SaveChickenActionDto>>> Search([FromBody] SaveChickenActionSearch search)
        {
            var result = await _service.Search(search, SaveChickenActionIncludes.Default);
            var resultDto = new PaginationDto<SaveChickenActionDto>
            {
                Data = _mapper.mapper.Map<List<SaveChickenActionDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut]
        public async Task<ActionResult<SaveChickenActionDto>> Update([FromBody] SaveChickenActionDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenAction>(dto);
            var result = await _service.Update(model);
            var resultDto = _mapper.mapper.Map<SaveChickenActionDto>(result);
            return Ok(resultDto);
        }

        [HttpGet("{id}/chicken-agreements")]
        public async Task<IActionResult> ChickenAgreement(int id)
        {
            var saveChickenAction = await _service.Read(id);
            if (saveChickenAction == null) return NotFound();

            var dateForSaveChickenAgreement =
                SaveChickenAgreementHelper.GetAgreementDate(saveChickenAction.Dates);

            var search = new SaveChickenRequestSearch
            {
                SaveChickenActionIds = [id],
                PageSize = 2000

            };
            var saveChickenRequests = await _saveChickenRequestService.Search(search, SaveChickenRequestIncludes.Default);

            if (!saveChickenRequests.Data.Any())
                return NotFound("No requests found for this action.");

            using var memoryStream = new MemoryStream();

            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var request in saveChickenRequests.Data)
                {

                    var firstName = request.Person?.Contact?.FirstName?.Trim();
                    var lastName = request.Person?.Contact?.LastName?.Trim();
                    var overnemerName = string.Join("_", new[] { firstName, lastName }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                    var model = new ChickenHandoverModel
                    {
                        ChickenCount = request.NumberOfChickensToBeSaved,
                        RoosterCount = request.NumberOfRoostersToBeSaved,
                        OvernehmerName = SaveChickenAgreementHelper.GetOvernehmerName(request),
                        Date = dateForSaveChickenAgreement

                    };

                    // 1. Generate PDF bytes (YOU replace this)
                    byte[] pdfBytes = new ChickenHandoverDocument(model).GeneratePdf();

                    // 2. Create ZIP entry
                    var fileName = $"{model.OvernehmerName}_{request.Id}.pdf";

                    var entry = zip.CreateEntry(fileName, CompressionLevel.Optimal);

                    using var entryStream = entry.Open();
                    using var pdfStream = new MemoryStream(pdfBytes);

                    await pdfStream.CopyToAsync(entryStream);
                }
            }

            memoryStream.Position = 0;
            var bytes = memoryStream.ToArray();
            return File(bytes, "application/zip", "Abgabevereinbarungen.zip");
        }


        [HttpGet("{id}/meeting-locations-pdf")]
        public async Task<IActionResult> MeetingLocationsPdf(int id)
        {
            var saveChickenAction = await _service.Read(id);
            if (saveChickenAction == null) return NotFound();

            if (string.IsNullOrWhiteSpace(saveChickenAction.DriverRoutePlansJson))
                return NotFound("No saved route plans data found.");

            SavedRequestAssignmentsData? savedData;
            try
            {
                savedData = JsonSerializer.Deserialize<SavedRequestAssignmentsData>(saveChickenAction.DriverRoutePlansJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading saved assignments: {ex.Message}");
                return BadRequest("Could not parse saved route plan data.");
            }

            if (savedData == null || savedData.MeetingLocations == null || savedData.Assignments == null)
                return NotFound("No saved assignments data found.");

            var meetingLocations = savedData.MeetingLocations;
            var assignments = savedData.Assignments;

            var search = new SaveChickenRequestSearch
            {
                SaveChickenActionIds = [id],
                PageSize = 2000
            };
            var saveChickenRequests = await _saveChickenRequestService.Search(search, SaveChickenRequestIncludes.Default);

            if (!saveChickenRequests.Data.Any())
                return NotFound("No requests found for this action.");

            var requestNamesById = saveChickenRequests.Data.ToDictionary(
                r => r.Id,
                r => $"{r.Person?.Contact?.FirstName} {r.Person?.Contact?.LastName}".Trim());

            // requestId -> (locationId, order), mirrors the Blazor component's assignment lookup
            var assignmentsByRequestId = assignments
                .GroupBy(a => a.RequestId)
                .ToDictionary(g => g.Key, g => g.First());

            var model = new MeetingLocationScheduleModel();

            var dayGroups = meetingLocations
                .GroupBy(l => l.DateTime.Date)
                .OrderBy(g => g.Key);

            var saveChickenRequestsById = saveChickenRequests.Data.ToDictionary(r => r.Id);


            foreach (var dayGroup in dayGroups)
            {
                var dayEntry = new MeetingLocationScheduleDayGroup { Date = dayGroup.Key };
                var orderedLocations = dayGroup.OrderBy(l => l.DateTime).ToList();

                for (int i = 0; i < orderedLocations.Count; i++)
                {
                    var location = orderedLocations[i];

                    var assignedRequests = assignmentsByRequestId.Values
                        .Where(a => a.MeetingLocationId == location.Id)
                        .OrderBy(a => a.Order)
                        .Select(a =>
                        {
                            saveChickenRequestsById.TryGetValue(a.RequestId, out var request);
                            return new AssignedRequestEntry
                            {
                                ChickenCount = request?.NumberOfChickensToBeSaved ?? 0,
                                RoosterCount = request?.NumberOfRoostersToBeSaved ?? 0,
                                Name = $"{request?.Person?.Contact?.FirstName} {request?.Person?.Contact?.LastName}".Trim(),
                                Phone = request?.Person?.Contact?.PhoneNumber ?? ""
                            };
                        })
                        .ToList();

                    dayEntry.Locations.Add(new MeetingLocationScheduleEntry
                    {
                        SequenceInDay = i + 1,
                        Name = location.Name,
                        DateTime = location.DateTime,
                        AssignedRequests = assignedRequests
                    });
                }

                model.Days.Add(dayEntry);
            }

            byte[] pdfBytes = new MeetingLocationScheduleDocument(model).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Treffpunkte.pdf");
        }
    }
}
