using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.DtosImpl
{
    public class BlackListedPersonDto : IDto
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
        public string GenericName { get; set; } = string.Empty;

        public BlackListedPersonDto()
        {
            Contact = new ContactDto();
            Address = new AddressDto();
        }
    }
}
