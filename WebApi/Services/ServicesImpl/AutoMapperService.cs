using AutoMapper;
using Shared.Dtos.DtosImpl;
using WebApi.Models.ModelsImpl;

public class AutoMapperService
{

    public IMapper mapper { get; set; }

    public AutoMapperService()
    {

        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<Contact, ContactDto>().ReverseMap();
            cfg.CreateMap<Address, AddressDto>().ReverseMap();
            cfg.CreateMap<Farm, FarmDto>().ReverseMap();
            cfg.CreateMap<SaveChickenRequest, SaveChickenRequestDto>().ReverseMap();
            cfg.CreateMap<SaveChickenAction, SaveChickenActionDto>().ReverseMap();
            cfg.CreateMap<StoredFile, StoredFileDto>().ReverseMap();
            cfg.CreateMap<Driver, DriverDto>().ReverseMap();
        });

        mapper = new Mapper(config);
    }
}
