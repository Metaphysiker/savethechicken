
using WebApi.Interfaces;

namespace WebApi.Models.ModelsImpl
{
    public class Farm : IModel, IEntityWithAddress
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int NumberOfChickens { get; set; } = 0;
        public string Size { get; set; } = String.Empty;
        public string Color { get; set; } = String.Empty;
        public List<DateOnly> DatesForRescues { get; set; } = new List<DateOnly>();
        public string GeneralInformation { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public Contact Contact { get; set; } = null!;
        public int ContactId { get; set; }
        public Address Address { get; set; } = null!;
        public int AddressId { get; set; }
        public List<StoredFile> Files { get; set; }

        Farm()
        {
            Files = new List<StoredFile>();
            Contact = new Contact();
            Address = new Address();
        }
    }
}
