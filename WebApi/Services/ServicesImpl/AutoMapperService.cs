
using AutoMapper;
using Shared.Dtos.DtosImpl;
using WebApi.Models.ModelsImpl;

public class AutoMapperService
{

    public IMapper mapper { get; set; }

    public AutoMapperService(ILoggerFactory loggerFactory)
    {
        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<SaveChickenRequest, SaveChickenRequestDto>();
        }, loggerFactory);

        mapper = new Mapper(config);
    }
}
