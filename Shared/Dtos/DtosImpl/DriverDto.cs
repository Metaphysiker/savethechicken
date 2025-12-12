using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    public class DriverDto : IDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ValidateComplexType]
        [Required]
        public ContactDto Contact { get; set; } = new ContactDto();
        public int ContactId { get; set; }

        [ValidateComplexType]
        public AddressDto Address { get; set; } = new AddressDto();
        public int AddressId { get; set; }
        public String CarMake { get; set; } = String.Empty;
        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();
    }
}


