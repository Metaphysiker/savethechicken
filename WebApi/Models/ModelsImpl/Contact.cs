namespace WebApi.Models.ModelsImpl
{
    public class Contact : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public String FirstName { get; set; } = String.Empty;
        public String LastName { get; set; } = String.Empty;
        public String PhoneNumber { get; set; } = String.Empty;
        public String Email { get; set; } = String.Empty;
        public List<ContactCategory> Categories { get; set; } = new List<ContactCategory>();
        public String CarMake { get; set; } = String.Empty;
        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();
    }
}
