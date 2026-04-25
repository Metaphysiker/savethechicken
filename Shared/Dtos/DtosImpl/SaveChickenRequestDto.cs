using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Shared.Dtos.DtosImpl
{
    public class SaveChickenRequestDto : IDto, IEntityWithFileDtos, IEntityWithSaveChickenActionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int? PersonId { get; set; }

        public int NumberOfChickensToBeSaved { get; set; } = 0;
        public int NumberOfRoostersToBeSaved { get; set; } = 0;


        [Required]
        public string DescriptionOfPlaceForChickens { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTermsAndConditions { get; set; } = false;

        public bool AlreadyReceivedChickenPreviously { get; set; } = false;

        [Required(ErrorMessage = "You must accept")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept")]
        public bool ConfirmThatIFulfillCriteria { get; set; } = false;

        public string Message { get; set; } = string.Empty;
        public SaveChickenActionDto? SaveChickenAction { get; set; }
        public int? SaveChickenActionId { get; set; }

        public int NumberOfBoxes { get; set; } = 0;

        public string Color { get; set; } = String.Empty;

        public List<StoredFileDto> Files { get; set; }

        public List<int> BlackListedPersonIds { get; set; } = new List<int>();

        public PersonDto? Person { get; set; }

        public bool IsSubmittedFromPublicForm { get; set; } = false;
        public bool IsHandled { get; set; } = false;
        public bool IsArchived { get; set; } = false;

        public string GenericName { get; set; } = string.Empty;

        public SaveChickenRequestDto()
        {
            Files = new List<StoredFileDto>();
        }

    }
}
