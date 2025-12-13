
namespace WebApi.Models.ModelsImpl
{
    public class File : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string FileName { get; set; } = String.Empty;
        public string ContentType { get; set; } = String.Empty;
        public string FileKey { get; set; } = String.Empty;

        public int? FarmId { get; set; }
        public Farm? Farm { get; set; }

        // Optional foreign key and navigation property for SaveChickenRequest
        public int? SaveChickenRequestId { get; set; }
        public SaveChickenRequest? SaveChickenRequest { get; set; }
    }
}
