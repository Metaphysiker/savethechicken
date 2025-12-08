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

        [ValidateComplexType]

        public ContactDto Contact { get; set; } = new ContactDto();

        [Range(1, int.MaxValue, ErrorMessage = "Number of chickens must be greater than 0")]
        public int NumberOfChickensToBeSaved { get; set; } = 0;

        [Required]
        public string DescriptionOfPlaceForChickens { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTermsAndConditions { get; set; } = false;

        public string Message { get; set; } = string.Empty;
        public int? SaveChickenActionId { get; set; }
    }
}
