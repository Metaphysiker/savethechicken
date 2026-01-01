using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    public class ContactDto : IDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Required]
        public String FirstName { get; set; } = String.Empty;

        [Required]
        public String LastName { get; set; } = String.Empty;

        [Required]
        [Phone]
        public String PhoneNumber { get; set; } = String.Empty;

        [Required]
        [EmailAddress]
        public String Email { get; set; } = String.Empty;
        public List<ContactCategory> Categories { get; set; } = new List<ContactCategory>();
        public String CarMake { get; set; } = String.Empty;
        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();
        public string GenericName { get; set; } = String.Empty;
    }
}
