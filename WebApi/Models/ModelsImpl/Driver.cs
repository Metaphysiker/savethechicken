namespace WebApi.Models.ModelsImpl
{
    public class Driver : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Contact Contact { get; set; }
        public int ContactId { get; set; }
        public Address Address { get; set; }
        public int AddressId { get; set; }
        public String CarMake { get; set; } = String.Empty;
        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();
    }
}
