using WebApi.Classes;

namespace WebApi.Models.ModelsImpl
{
    public class SaveChickenRequest : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Contact Contact { get; set; } = new Contact();
        public int NumberOfChickensToBeSaved { get; set; } = 0;
        public String DescriptionOfPlaceForChickens { get; set; } = String.Empty;
        public bool AcceptTermsAndConditions { get; set; } = false;
        public String Message { get; set; } = String.Empty;
        public int? SaveChickenActionId { get; set; }
        public SaveChickenAction? SaveChickenAction { get; set; }
    }
}
