using AutoMapper;
using Shared.Dtos.DtosImpl;
using WebApi.Models.ModelsImpl;

public class AutoMapperService
{

    public IMapper mapper { get; set; }

    public AutoMapperService()
    {

        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<Contact, ContactDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}, {src.Email}".Trim()))
                .ReverseMap();
            cfg.CreateMap<Address, AddressDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => $"{src.Street} {src.City} {src.PostalCode}".Trim()))
                .ReverseMap();
            cfg.CreateMap<Farm, FarmDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => $"{src.Name}, {src.Contact.FirstName} {src.Contact.LastName}, {src.Address.City}".Trim()))
                .ReverseMap();
            cfg.CreateMap<SaveChickenRequest, SaveChickenRequestDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => $"{src.Contact.FirstName} {src.Contact.LastName}, {src.Contact.Email}, {src.Address.City}".Trim()))
                .ReverseMap();
            cfg.CreateMap<SaveChickenAction, SaveChickenActionDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => src.Title))
                .ReverseMap();
            cfg.CreateMap<StoredFile, StoredFileDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => src.FileName))
                .ReverseMap();
            cfg.CreateMap<Driver, DriverDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => $"{src.Contact.FirstName} {src.Contact.LastName}".Trim()))
                .ReverseMap();
            cfg.CreateMap<BlackListedPerson, BlackListedPersonDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => $"{src.Contact.FirstName} {src.Contact.LastName}".Trim()))
                .ReverseMap();
        });

        mapper = new Mapper(config);
    }
}
