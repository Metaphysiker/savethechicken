using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using System.Linq.Expressions;
using WebApi.Database.Includes;
using WebApi.Factories;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;
using WebApi.Services;
using WebApi.Services.ServicesImpl;

namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class PersonController : ControllerBase, IModelController<PersonDto, PersonSearch>
    {
        private readonly GenericModelService<Person, PersonSearch> _service;
        private readonly AutoMapperService _mapper;
        private readonly ILogger<PersonController> _logger;

        public PersonController(
            GenericModelServiceFactory genericModelServiceFactory,
            AutoMapperService mapper,
            ILogger<PersonController> logger)
        {
            _service = genericModelServiceFactory.Create<Person, PersonSearch>();
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<PersonDto>> Create([FromBody] PersonDto dto)
        {
            var model = _mapper.mapper.Map<Person>(dto);
            var result = await _service.Create(model, PersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<PersonDto>(result);
            return CreatedAtAction(nameof(Read), new { id = resultDto.Id }, resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDto>> Read(int id)
        {
            var result = await _service.Read(id, PersonIncludes.Default);
            if (result == null) return NotFound();
            var resultDto = _mapper.mapper.Map<PersonDto>(result);
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<PersonDto>>> ReadAll()
        {
            var result = await _service.ReadAll(PersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<List<PersonDto>>(result);
            return Ok(resultDto);
        }

        [HttpPost("search")]
        public async Task<ActionResult<PaginationDto<PersonDto>>> Search([FromBody] PersonSearch search)
        {
            var result = await _service.Search(search, PersonIncludes.Default);
            var resultDto = new PaginationDto<PersonDto>
            {
                Data = _mapper.mapper.Map<List<PersonDto>>(result.Data),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
            return Ok(resultDto);
        }

        [HttpPut]
        public async Task<ActionResult<PersonDto>> Update([FromBody] PersonDto dto)
        {
            var model = _mapper.mapper.Map<Person>(dto);
            var result = await _service.Update(model, PersonIncludes.Default);
            var resultDto = _mapper.mapper.Map<PersonDto>(result);
            return Ok(resultDto);
        }
    }
}
