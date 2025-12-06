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
    public class SaveChickenActionController : ControllerBase, IModelController<SaveChickenActionDto, ISearchDto>
    {
        private readonly GenericModelService<SaveChickenAction, ISearchDto> _service;
        private readonly AutoMapperService _mapper;

        public SaveChickenActionController(GenericModelServiceFactory genericModelServiceFactory, AutoMapperService mapper)
        {
            _service = genericModelServiceFactory.Create<SaveChickenAction, ISearchDto>();
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaveChickenActionDto>> Read(int id)
        {
            var result = await _service.Read(id);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<SaveChickenActionDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<SaveChickenActionDto>>> ReadAll()
        {
            var result = await _service.ReadAll();
            var resultDto = _mapper.mapper.Map<List<SaveChickenActionDto>>(result);
            return Ok(resultDto);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<SaveChickenActionDto>>> Search([FromBody] ISearchDto search)
        {
            var result = await _service.Search(search);
            var resultDto = _mapper.mapper.Map<PaginationDto<SaveChickenActionDto>>(result);
            return Ok(resultDto);
        }

        [HttpPut]
        public async Task<ActionResult<SaveChickenActionDto>> Update([FromBody] SaveChickenActionDto dto)
        {
            var model = _mapper.mapper.Map<SaveChickenAction>(dto);
            var result = await _service.Update(model);
            var resultDto = _mapper.mapper.Map<SaveChickenActionDto>(result);
            return Ok(resultDto);
        }
    }
}
