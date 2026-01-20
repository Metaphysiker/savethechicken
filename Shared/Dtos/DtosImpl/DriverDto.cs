using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    public class DriverDto : IDto, IEntityWithFileDtos, IEntityWithSaveChickenActionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ValidateComplexType]
        [Required]
        public ContactDto Contact { get; set; }
        public int ContactId { get; set; }

        [ValidateComplexType]
        public AddressDto Address { get; set; }
        public int AddressId { get; set; }
        public String CarMake { get; set; } = String.Empty;
        public String Message { get; set; } = String.Empty;

        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();
        public List<StoredFileDto> Files { get; set; }
        public SaveChickenActionDto? SaveChickenAction { get; set; }
        public int? SaveChickenActionId { get; set; }
        public string GenericName { get; set; } = string.Empty;
        public int CapacityForChickens { get; set; } = 20;

        public DriverDto()
        {
            Contact = new ContactDto();
            Address = new AddressDto();
            Files = new List<StoredFileDto>();
        }
    }
}


