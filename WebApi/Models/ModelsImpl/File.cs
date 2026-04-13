
namespace WebApi.Models.ModelsImpl
{
    public class StoredFile : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string FileName { get; set; } = String.Empty;
        public string ContentType { get; set; } = String.Empty;
        public string FileKey { get; set; } = String.Empty;

        public int? FarmId { get; set; }
        public Farm? Farm { get; set; }

        public int? SaveChickenRequestId { get; set; }
        public SaveChickenRequest? SaveChickenRequest { get; set; }

        public int? SaveChickenDriveRequestId { get; set; }
        public SaveChickenDriveRequest? SaveChickenDriveRequest { get; set; }
    }
}
