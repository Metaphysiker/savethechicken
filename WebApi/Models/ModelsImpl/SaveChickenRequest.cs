using WebApi.Classes;

namespace WebApi.Models.ModelsImpl
{
    public class SaveChickenRequest : IModel
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
        public String DescriptionOfPlaceForChickens { get; set; } = String.Empty;
        public bool AcceptTermsAndConditions { get; set; } = false;
        public String Message { get; set; } = String.Empty;
        public int? SaveChickenActionId { get; set; }
        public SaveChickenAction? SaveChickenAction { get; set; }
        public DateOnly DateForHandOver { get; set; } = new DateOnly();
        public string Color { get; set; } = String.Empty;
    }
}
