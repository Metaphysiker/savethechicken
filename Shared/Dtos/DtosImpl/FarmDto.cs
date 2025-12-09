using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.DtosImpl
{
    public class FarmDto :IDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Number of chickens must be greater than 0")]
        public int NumberOfChickens { get; set; } = 0;
        public string Size { get; set; } = String.Empty;
        public string Color { get; set; } = String.Empty;
        // at least one date required
        [MinLength(1, ErrorMessage = "At least one date for rescue is required")]
        public List<DateOnly> DatesForRescues { get; set; } = new List<DateOnly>();
        public string GeneralInformation { get; set; } = String.Empty;
        [Required]
        public string Name { get; set; } = String.Empty;
        [ValidateComplexType]
        [Required]
        public ContactDto Contact { get; set; }
        public int ContactId { get; set; }
        [ValidateComplexType]
        [Required]
        public AddressDto Address { get; set; }
        public int AddressId { get; set; }
    }
}
