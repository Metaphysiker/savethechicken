using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    public class ContactDto : IDto
    {
        [Required]
        public String FirstName { get; set; } = String.Empty;
        [Required]
        public String LastName { get; set; } = String.Empty;
        [Required]
        public String Street { get; set; } = String.Empty;
        [Required]
        public String City { get; set; } = String.Empty;
        [Required]
        public String PostalCode { get; set; } = String.Empty;
        [Required]
        [Phone]
        public String PhoneNumber { get; set; } = String.Empty;
        [Required]
        [EmailAddress]
        public String Email { get; set; } = String.Empty;
    }
}
