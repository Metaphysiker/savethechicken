using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.DtosImpl
{
    public class SaveChickenDriveRequestDto : IDto, IEntityWithFileDtos, IEntityWithSaveChickenActionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int? PersonId { get; set; }
        public PersonDto? Person { get; set; }

        [Required]
        public string CarMake { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int CapacityForChickens { get; set; } = 20;

        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();

        public SaveChickenActionDto? SaveChickenAction { get; set; }
        public int? SaveChickenActionId { get; set; }

        public List<StoredFileDto> Files { get; set; }
        public bool IsSubmittedFromPublicForm { get; set; } = false;
        public bool IsHandled { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public string GenericName { get; set; } = string.Empty;

        public SaveChickenDriveRequestDto()
        {
            Files = new List<StoredFileDto>();
        }
    }
}
