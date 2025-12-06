using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using WebApi.Factories;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;

namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaveChickenRequestController : ControllerBase, IModelController<SaveChickenRequestDto, SaveChickenRequestSearch>
    {
        private readonly GenericModelService<SaveChickenRequest, SaveChickenRequestSearch> _service;
        private readonly AutoMapperService _mapper;

        public SaveChickenRequestController(GenericModelServiceFactory genericModelServiceFactory, AutoMapperService mapper)
        {
            _service = genericModelServiceFactory.Create<SaveChickenRequest, SaveChickenRequestSearch>();
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<SaveChickenRequestDto>> Create([FromBody] SaveChickenRequestDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenRequest>(dto);
            var result = await _service.Create(model);
            var resultDto = _mapper.mapper.Map<SaveChickenRequestDto>(result);
            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaveChickenRequestDto>> Read(int id)
        {
            var result = await _service.Read(id);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<SaveChickenRequestDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<SaveChickenRequestDto>>> ReadAll()
        {
            var result = await _service.ReadAll();
            var resultDto = _mapper.mapper.Map<List<SaveChickenRequestDto>>(result);
            return Ok(resultDto);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<SaveChickenRequestDto>>> Search([FromBody] SaveChickenRequestSearch search)
        {
            var result = await _service.Search(search);
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

        [HttpPut]
        public async Task<ActionResult<SaveChickenRequestDto>> Update([FromBody] SaveChickenRequestDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenRequest>(dto);
            var result = await _service.Update(model);
            var resultDto = _mapper.mapper.Map<SaveChickenRequestDto>(result);
            return Ok(resultDto);
        }

    }
}
