using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    /// <summary>
    /// DTO for public users submitting a SaveChickenRequest.
    /// Contains embedded contact and address information since the person doesn't exist yet.
    /// </summary>
    public class SaveChickenRequestPublicDto : IEntityWithFileDtos
    {
        // Contact Information
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Address Information
        [Required]
        public string Street { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string PostalCode { get; set; } = string.Empty;

        // SaveChickenRequest Information
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

        public int? SaveChickenActionId { get; set; }

        public int NumberOfBoxes { get; set; } = 0;

        public string Color { get; set; } = string.Empty;

        public bool IsSubmittedFromPublicForm { get; set; } = true;
        public bool IsHandled { get; set; } = false;
        public List<StoredFileDto> Files { get; set; }

        public SaveChickenRequestPublicDto()
        {
            Files = new List<StoredFileDto>();
        }
    }
}
