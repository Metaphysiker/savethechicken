using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using WebApi.Database.Includes;
using WebApi.Factories;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;

namespace WebApi.Controllers.ControllersImpl
{
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ApiController]
    [Route("api/[controller]")]
    public class BlackListedPersonController : ControllerBase, IModelController<BlackListedPersonDto, BlackListedPersonSearch>
    {
        private readonly GenericModelService<BlackListedPerson, BlackListedPersonSearch> _service;
        private readonly AutoMapperService _mapper;

        public BlackListedPersonController(GenericModelServiceFactory genericModelServiceFactory, AutoMapperService mapper)
        {
            _service = genericModelServiceFactory.Create<BlackListedPerson, BlackListedPersonSearch>();
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<BlackListedPersonDto>> Create([FromBody] BlackListedPersonDto dto)
        {
            var model = _mapper.mapper.Map<BlackListedPerson>(dto);
            var result = await _service.Create(model, BlackListedPersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<BlackListedPersonDto>(result);
            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BlackListedPersonDto>> Read(int id)
        {
            var result = await _service.Read(id, BlackListedPersonIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<BlackListedPersonDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<BlackListedPersonDto>>> ReadAll()
        {
            var result = await _service.ReadAll(BlackListedPersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<BlackListedPersonDto>>(result);
            return Ok(resultDto);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<BlackListedPersonDto>>> Search([FromBody] BlackListedPersonSearch search)
        {
            var result = await _service.Search(search, BlackListedPersonIncludes.Default);
            var resultDto = new PaginationDto<BlackListedPersonDto>
            {
                Data = _mapper.mapper.Map<List<BlackListedPersonDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [HttpPut]
        public async Task<ActionResult<BlackListedPersonDto>> Update([FromBody] BlackListedPersonDto dto)
        {
            var model = _mapper.mapper.Map<BlackListedPerson>(dto);
            var result = await _service.Update(model, BlackListedPersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<BlackListedPersonDto>(result);
            return Ok(resultDto);
        }
    }
}
