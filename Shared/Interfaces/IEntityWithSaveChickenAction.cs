using Shared.Dtos.DtosImpl;

public interface IEntityWithSaveChickenActionDto
{
        public SaveChickenActionDto? SaveChickenAction { get; set; }
        public int? SaveChickenActionId { get; set; }
}
