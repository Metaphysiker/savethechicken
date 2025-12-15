using WebApi.Models.ModelsImpl;

namespace WebApi.Interfaces
{
    public interface IEntityWithAddress
    {
        public Address Address { get; set; }
    }
}
