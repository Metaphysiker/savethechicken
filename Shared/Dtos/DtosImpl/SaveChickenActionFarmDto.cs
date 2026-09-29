namespace Shared.Dtos.DtosImpl;

public class SaveChickenActionFarmDto : IDto
{
    public int Id { get; set; }

    public string GenericName { get; set; } = string.Empty;

    public int FarmId { get; set; }
    public FarmDto? Farm { get; set; }

    public int SaveChickenActionId { get; set; }
    public SaveChickenActionDto? SaveChickenAction { get; set; }

    public int NumberOfChickensToBeSaved { get; set; } = 0;
    public int NumberOfRoostersToBeSaved { get; set; } = 0;
}
