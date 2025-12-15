using WebApi.Interfaces;
using WebApi.Models.ModelsImpl;

namespace WebApi.Models.ModelsImpl
{
    public class SaveChickenRequest : IModel, IEntityWithAddress
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Contact Contact { get; set; }
        public int ContactId { get; set; }
        public Address Address { get; set; }
        public int AddressId { get; set; }
        public bool IsHandoverAtDifferentAddress { get; set; } = false;
        public Address? AddressForHandOver { get; set; }
        public int? AddressForHandOverId { get; set; }
        public int NumberOfChickensToBeSaved { get; set; } = 0;
        public int NumberOfBoxes { get; set; } = 0;
        public string DescriptionOfPlaceForChickens { get; set; } = string.Empty;
        public bool AcceptTermsAndConditions { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public int? SaveChickenActionId { get; set; }
        public SaveChickenAction? SaveChickenAction { get; set; }
        public List<DateOnly> DatesForHandOver { get; set; } = new List<DateOnly>();
        public string Color { get; set; } = string.Empty;
        public List<StoredFile> Files { get; set; }

        SaveChickenRequest()
        {
            Contact = new Contact();
            Address = new Address();
            Files = new List<StoredFile>();
        }
    }
}
