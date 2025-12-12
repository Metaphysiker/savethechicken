namespace WebApi.Models.ModelsImpl
{
    public class Address : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public String Street { get; set; } = String.Empty;
        public String City { get; set; } = String.Empty;
        public String PostalCode { get; set; } = String.Empty;
    }
}
