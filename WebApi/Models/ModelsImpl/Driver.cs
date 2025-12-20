using WebApi.Interfaces;

namespace WebApi.Models.ModelsImpl
{
    public class Driver : IModel, IEntityWithAddress, IEntityWithFiles
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
        public List<StoredFile> Files { get; set; }
        public int? SaveChickenActionId { get; set; }
        public SaveChickenAction? SaveChickenAction { get; set; }

        public Driver()
        {
            Contact = new Contact();
            Address = new Address();
            Files = new List<StoredFile>();
        }
    }
}
