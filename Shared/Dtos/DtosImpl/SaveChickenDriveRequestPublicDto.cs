using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    /// <summary>
    /// DTO for public users submitting a drive request.
    /// Contains embedded contact and address information since the person doesn't exist yet.
    /// </summary>
    public class SaveChickenDriveRequestPublicDto
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

        // Drive Request Information
        [Required]
        public string CarMake { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int CapacityForChickens { get; set; } = 20;

        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();

        public int? SaveChickenActionId { get; set; }
    }
}
