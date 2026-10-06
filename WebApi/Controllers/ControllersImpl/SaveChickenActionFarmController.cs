using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using WebApi.Factories;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;
using Shared.Classes;
using WebApi.Interfaces;
using WebApi.Database.Includes;

namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaveChickenActionFarmController : ControllerBase, IModelController<SaveChickenActionFarmDto, SaveChickenActionFarmSearch>
    {
        private readonly GenericModelService<SaveChickenActionFarm, SaveChickenActionFarmSearch> _service;
        private readonly AutoMapperService _mapper;

        public SaveChickenActionFarmController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper)
        {
            _service = genericModelServiceFactory.Create<SaveChickenActionFarm, SaveChickenActionFarmSearch>();
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<SaveChickenActionFarmDto>> Create([FromBody] SaveChickenActionFarmDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenActionFarm>(dto);
            var result = await _service.Create(model, SaveChickenActionFarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenActionFarmDto>(result);

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
        public async Task<ActionResult<SaveChickenActionFarmDto>> Read(int id)
        {
            var result = await _service.Read(id, SaveChickenActionFarmIncludes.Default);
            if (result == null) return NotFound();

            var resultDto = _mapper.mapper.Map<SaveChickenActionFarmDto>(result);
            return Ok(resultDto);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<SaveChickenActionFarmDto>>> ReadAll()
        {
            var result = await _service.ReadAll(SaveChickenActionFarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<SaveChickenActionFarmDto>>(result);
            return Ok(resultDto);
        }

        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<SaveChickenActionFarmDto>>> Search([FromBody] SaveChickenActionFarmSearch search)
        {
            var result = await _service.Search(search, SaveChickenActionFarmIncludes.Default);
            var resultDto = new PaginationDto<SaveChickenActionFarmDto>
            {
                Data = _mapper.mapper.Map<List<SaveChickenActionFarmDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut]
        public async Task<ActionResult<SaveChickenActionFarmDto>> Update([FromBody] SaveChickenActionFarmDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenActionFarm>(dto);
            var result = await _service.Update(model, SaveChickenActionFarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<SaveChickenActionFarmDto>(result);
            return Ok(resultDto);
        }
    }
}
