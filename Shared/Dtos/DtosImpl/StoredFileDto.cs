using Shared.Dtos;
using Shared.Dtos.DtosImpl;

public class StoredFileDto : IDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string FileName { get; set; } = String.Empty;
        public string ContentType { get; set; } = String.Empty;
        public string FileKey { get; set; } = String.Empty;

        public int? FarmId { get; set; }
        public FarmDto? Farm { get; set; }
        public int? SaveChickenRequestId { get; set; }
        public SaveChickenRequestDto? SaveChickenRequest { get; set; }
    }
