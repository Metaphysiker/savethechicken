using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using WebApi.Database.Includes;
using WebApi.Factories;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;

namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase, IModelController<DriverDto, DriverSearch>
    {
        private readonly GenericModelService<Driver, DriverSearch> _service;
        private readonly AutoMapperService _mapper;

        public DriverController(GenericModelServiceFactory genericModelServiceFactory, AutoMapperService mapper)
        {
            _service = genericModelServiceFactory.Create<Driver, DriverSearch>();
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<DriverDto>> Create([FromBody] DriverDto dto)
        {
            var model = _mapper.mapper.Map<Driver>(dto);
            var result = await _service.Create(model, DriverIncludes.Default);
            var resultDto = _mapper.mapper.Map<DriverDto>(result);
            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DriverDto>> Read(int id)
        {
            var result = await _service.Read(id, DriverIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<DriverDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<DriverDto>>> ReadAll()
        {
            var result = await _service.ReadAll(DriverIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<DriverDto>>(result);
            return Ok(resultDto);
        }

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

        [HttpPut]
        public async Task<ActionResult<DriverDto>> Update([FromBody] DriverDto dto)
        {
            var model = _mapper.mapper.Map<Driver>(dto);
            var result = await _service.Update(model, DriverIncludes.Default);
            var resultDto = _mapper.mapper.Map<DriverDto>(result);
            return Ok(resultDto);
        }
    }
}
