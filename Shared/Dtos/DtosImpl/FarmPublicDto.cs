using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    /// <summary>
    /// DTO for public users registering a farm.
    /// Contains embedded contact and address information since the contact doesn't exist yet.
    /// </summary>
    public class FarmPublicDto
    {
        // Farm Information
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public string GeneralInformation { get; set; } = string.Empty;

        public int? SaveChickenActionId { get; set; }

        public bool IsSubmittedFromPublicForm { get; set; } = true;
        public bool IsHandled { get; set; } = false;

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
    }
}
