namespace WebApi.Models.ModelsImpl
{
    public class SaveChickenRequest : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public String FirstName { get; set; } = String.Empty;
        public String LastName { get; set; } = String.Empty;
        public String Street { get; set; } = String.Empty;
        public String City { get; set; } = String.Empty;
        public String PostalCode { get; set; } = String.Empty;
        public String PhoneNumber { get; set; } = String.Empty;
        public String Email { get; set; } = String.Empty;
        public int NumberOfChickensToBeSaved { get; set; } = 0;
        public String DescriptionOfPlaceForChickens { get; set; } = String.Empty;
        public bool AcceptTermsAndConditions { get; set; } = false;
        public String Message { get; set; } = String.Empty;
    }
}
