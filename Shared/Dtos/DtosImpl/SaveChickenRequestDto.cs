using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Dtos.DtosImpl
{
    public class SaveChickenRequestDto : IDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int NumberOfChickensToBeSaved { get; set; } = 0;
        public string DescriptionOfPlaceForChickens { get; set; } = string.Empty;
        public bool AcceptTermsAndConditions { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }
}
