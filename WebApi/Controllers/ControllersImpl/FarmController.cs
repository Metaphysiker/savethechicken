using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using System.Linq.Expressions;
using WebApi.Database.Includes;
using WebApi.Factories;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;

namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmController : ControllerBase, IModelController<FarmDto, FarmSearch>
    {
        private readonly GenericModelService<Farm, FarmSearch> _service;
        private readonly AutoMapperService _mapper;

        public FarmController(GenericModelServiceFactory genericModelServiceFactory, AutoMapperService mapper)
        {
            _service = genericModelServiceFactory.Create<Farm, FarmSearch>();
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<FarmDto>> Create([FromBody] FarmDto dto)
        {
            var model = _mapper.mapper.Map<Farm>(dto);
            var result = await _service.Create(model, FarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<FarmDto>(result);
            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FarmDto>> Read(int id)
        {
            var result = await _service.Read(id, FarmIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<FarmDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<FarmDto>>> ReadAll()
        {
            var result = await _service.ReadAll(FarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<FarmDto>>(result);
            return Ok(resultDto);
        }

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

        [HttpPut]
        public async Task<ActionResult<FarmDto>> Update([FromBody] FarmDto dto)
        {
            var model = _mapper.mapper.Map<Farm>(dto);
            var result = await _service.Update(model, FarmIncludes.Default);
            var resultDto = _mapper.mapper.Map<FarmDto>(result);
            return Ok(resultDto);
        }

    }
}
