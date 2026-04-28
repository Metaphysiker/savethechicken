using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.DtosImpl
{
    public class FarmDto :IDto, IEntityWithFileDtos, IEntityWithSaveChickenActionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Number of chickens must be greater than 0")]
        public int NumberOfChickens { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Number of roosters must be greater than or equal to 0")]
        public int NumberOfRoosters { get; set; } = 0;
        public string Size { get; set; } = String.Empty;
        public string Color { get; set; } = String.Empty;
        public string GeneralInformation { get; set; } = String.Empty;
        [Required]
        public string Name { get; set; } = String.Empty;
        [ValidateComplexType]
        public ContactDto Contact { get; set; }
        public int ContactId { get; set; }
        [ValidateComplexType]
        public AddressDto Address { get; set; }
        public int AddressId { get; set; }
        public List<StoredFileDto> Files { get; set; }
        public SaveChickenActionDto? SaveChickenAction { get; set; }
        public int? SaveChickenActionId { get; set; }
        public bool IsSubmittedFromPublicForm { get; set; } = false;
        public bool IsHandled { get; set; } = false;
        public string GenericName { get; set; } = string.Empty;

        public FarmDto()
        {
            Files = new List<StoredFileDto>();
            Contact = new ContactDto();
            Address = new AddressDto();
        }
    }
}
