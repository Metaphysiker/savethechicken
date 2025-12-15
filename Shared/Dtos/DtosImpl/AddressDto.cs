using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.DtosImpl
{
    public class AddressDto : IDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Required]
        public String Street { get; set; } = String.Empty;
        [Required]
        public String City { get; set; } = String.Empty;
        [Required]
        public String PostalCode { get; set; } = String.Empty;
        public GeoCoordinate? GeoCoordinate { get; set; }

    }
}
