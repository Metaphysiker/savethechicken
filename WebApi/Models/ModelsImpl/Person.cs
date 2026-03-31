using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using WebApi.Interfaces;

namespace WebApi.Models.ModelsImpl
{
    public class Person : IModel, IEntityWithAddress
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Contact Contact { get; set; } = null!;
        public int ContactId { get; set; }

        public Address Address { get; set; } = null!;
        public int AddressId { get; set; }

        public bool IsBlacklisted { get; set; } = false;

        public List<SaveChickenRequest> SaveChickenRequests { get; set; }
        public List<SaveChickenDriveRequest> SaveChickenDriveRequests { get; set; }

        [Column(TypeName = "tsvector")]
        public NpgsqlTsVector SearchVector { get; set; } = null!;

        public Person()
        {
            SaveChickenRequests = new List<SaveChickenRequest>();
            SaveChickenDriveRequests = new List<SaveChickenDriveRequest>();
        }
    }
}
