using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using WebApi.Interfaces;

namespace WebApi.Models.ModelsImpl
{
    public class SaveChickenDriveRequest : IModel, IEntityWithFiles
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;

        public string CarMake { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int CapacityForChickens { get; set; } = 20;
        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();

        public int? SaveChickenActionId { get; set; }
        public SaveChickenAction? SaveChickenAction { get; set; }

        public List<StoredFile> Files { get; set; }

        [Column(TypeName = "tsvector")]
        public NpgsqlTsVector SearchVector { get; set; } = null!;

        public SaveChickenDriveRequest()
        {
            Files = new List<StoredFile>();
        }
    }
}
