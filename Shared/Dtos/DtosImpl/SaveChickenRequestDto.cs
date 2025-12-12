using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Dtos.DtosImpl
{
    public class SaveChickenRequestDto : IDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Number of chickens must be greater than 0")]
        public int NumberOfChickensToBeSaved { get; set; } = 0;

        [Required]
        public string DescriptionOfPlaceForChickens { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTermsAndConditions { get; set; } = false;

        public string Message { get; set; } = string.Empty;
        public SaveChickenActionDto? SaveChickenAction { get; set; }
        public int? SaveChickenActionId { get; set; }

        [ValidateComplexType]
        [Required]
        public ContactDto Contact { get; set; } = new ContactDto();
        public int ContactId { get; set; }
        [ValidateComplexType]
        [Required]
        public AddressDto Address { get; set; } = new AddressDto();
        public int AddressId { get; set; }
        public bool IsHandoverAtDifferentAddress { get; set; } = false;

        [ValidateComplexType]
        public AddressDto? AddressForHandOver { get; set; }
        public int? AddressForHandOverId { get; set; }
        public int NumberOfBoxes { get; set; } = 0;

        public List<DateOnly> DateForHandOver { get; set; } = new List<DateOnly>();
        public string Color { get; set; } = String.Empty;

    }
}
