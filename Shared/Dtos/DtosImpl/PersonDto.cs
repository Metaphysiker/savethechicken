using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    public class PersonDto : IDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ValidateComplexType]
        [Required]
        public ContactDto Contact { get; set; }
        public int ContactId { get; set; }

        [ValidateComplexType]
        [Required]
        public AddressDto Address { get; set; }
        public int AddressId { get; set; }

        public bool IsBlacklisted { get; set; } = false;

        public List<SaveChickenRequestDto> SaveChickenRequests { get; set; }
        public List<SaveChickenDriveRequestDto> SaveChickenDriveRequests { get; set; }

        public string GenericName { get; set; } = string.Empty;

        public PersonDto()
        {
            Contact = new ContactDto();
            Address = new AddressDto();
            SaveChickenRequests = new List<SaveChickenRequestDto>();
            SaveChickenDriveRequests = new List<SaveChickenDriveRequestDto>();
        }
    }
}
