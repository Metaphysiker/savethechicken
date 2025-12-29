namespace WebApi.Models.ModelsImpl
{
    public class BlackListedPerson : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Contact Contact { get; set; }
        public int ContactId { get; set; }
        public Address Address { get; set; }
        public int AddressId { get; set; }

        public BlackListedPerson()
        {
            Contact = new Contact();
            Address = new Address();
        }
    }
}
