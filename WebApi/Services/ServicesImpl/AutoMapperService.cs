using AutoMapper;
using Shared.Dtos.DtosImpl;
using WebApi.Models.ModelsImpl;

public class AutoMapperService
{
    public IMapper mapper { get; }

    public AutoMapperService()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Contact
            cfg.CreateMap<Contact, ContactDto>()
                .ForMember(
                    dest => dest.GenericName,
                    opt => opt.MapFrom(src =>
                        $"{src.FirstName} {src.LastName}, {src.Email}".Trim()
                    )
                );

            cfg.CreateMap<ContactDto, Contact>();

            // Address
            cfg.CreateMap<Address, AddressDto>()
                .ForMember(
                    dest => dest.GenericName,
                    opt => opt.MapFrom(src =>
                        $"{src.Street} {src.City} {src.PostalCode}".Trim()
                    )
                );

            cfg.CreateMap<AddressDto, Address>();

            // Farm
            cfg.CreateMap<Farm, FarmDto>()
                .ForMember(
                    dest => dest.GenericName,
                    opt => opt.MapFrom(src =>
                        $"{src.Name}, {src.Contact.FirstName} {src.Contact.LastName}, {src.Address.City}".Trim()
                    )
                );

            cfg.CreateMap<FarmDto, Farm>();

            // SaveChickenRequest
            cfg.CreateMap<SaveChickenRequest, SaveChickenRequestDto>()
                .ForMember(
                    dest => dest.GenericName,
                    opt => opt.MapFrom(src =>
                        src.Person != null
                            ? $"{src.Person.Contact.FirstName} {src.Person.Contact.LastName}, {src.Person.Contact.Email}, {src.Person.Address.City}".Trim()
                            : string.Empty
                    )
                );

            cfg.CreateMap<SaveChickenRequestDto, SaveChickenRequest>()
                .ForMember(dest => dest.Person, opt => opt.Ignore()); // Ignore Person when mapping from DTO - use PersonId instead

            // SaveChickenAction
            cfg.CreateMap<SaveChickenAction, SaveChickenActionDto>()
                .ForMember(
                    dest => dest.GenericName,
                    opt => opt.MapFrom(src => src.Title)
                );

            cfg.CreateMap<SaveChickenActionDto, SaveChickenAction>();

            // StoredFile
            cfg.CreateMap<StoredFile, StoredFileDto>()
                .ForMember(dest => dest.GenericName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.SaveChickenRequest, opt => opt.Ignore())
                .ForMember(dest => dest.SaveChickenDriveRequest, opt => opt.Ignore())
                .ForMember(dest => dest.Farm, opt => opt.Ignore());

            cfg.CreateMap<StoredFileDto, StoredFile>();

            // Person
            cfg.CreateMap<Person, PersonDto>()
                .ForMember(
                    dest => dest.GenericName,
                    opt => opt.MapFrom(src =>
                        $"{src.Contact.FirstName} {src.Contact.LastName}".Trim()
                    )
                )
                .ForMember(dest => dest.SaveChickenRequests, opt => opt.Ignore())
                .ForMember(dest => dest.SaveChickenDriveRequests, opt => opt.Ignore());

            cfg.CreateMap<PersonDto, Person>();

            // SaveChickenDriveRequest
            cfg.CreateMap<SaveChickenDriveRequest, SaveChickenDriveRequestDto>()
                .ForMember(
                    dest => dest.GenericName,
                    opt => opt.MapFrom(src =>
                        $"{src.Person.Contact.FirstName} {src.Person.Contact.LastName}, {src.CarMake}".Trim()
                    )
                );

            cfg.CreateMap<SaveChickenDriveRequestDto, SaveChickenDriveRequest>()
                .ForMember(dest => dest.Person, opt => opt.Ignore()); // Ignore Person when mapping from DTO - use PersonId instead
        });

        mapper = config.CreateMapper();
    }
}
