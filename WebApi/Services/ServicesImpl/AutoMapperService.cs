using AutoMapper;
using Shared.Dtos.DtosImpl;
using WebApi.Classes;
using WebApi.Models.ModelsImpl;

public class AutoMapperService
{

    public IMapper mapper { get; set; }

    public AutoMapperService()
    {

        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<Contact, ContactDto>().ReverseMap();
            cfg.CreateMap<SaveChickenRequest, SaveChickenRequestDto>().ReverseMap();
            cfg.CreateMap<SaveChickenAction, SaveChickenActionDto>().ReverseMap();
        });

        mapper = new Mapper(config);
    }
}
