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

        [Required(ErrorMessage = "Vorname ist erforderlich.")]
        public string FirstName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Nachname ist erforderlich.")]
        public string LastName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Telefonnummer ist erforderlich.")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;


        [Required(ErrorMessage = "E-Mail ist erforderlich.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Address Information

        [Required(ErrorMessage = "Strasse und Hausnummer sind erforderlich.")]
        public string Street { get; set; } = string.Empty;


        [Required(ErrorMessage = "Ort ist erforderlich.")]
        public string City { get; set; } = string.Empty;


        [Required(ErrorMessage = "PLZ ist erforderlich.")]
        public string PostalCode { get; set; } = string.Empty;

        // Drive Request Information

        [Required(ErrorMessage = "Fahrzeugmarke und Modell sind erforderlich.")]
        public string CarMake { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int CapacityForChickens { get; set; } = 20;

        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();

        public int? SaveChickenActionId { get; set; }

        public bool IsSubmittedFromPublicForm { get; set; } = true;
        public bool IsHandled { get; set; } = false;

        [Required(ErrorMessage = "Sie müssen die Allgemeinen Geschäftsbedingungen akzeptieren.")]
        public bool AcceptTermsAndConditions { get; set; } = false;
    }
}
