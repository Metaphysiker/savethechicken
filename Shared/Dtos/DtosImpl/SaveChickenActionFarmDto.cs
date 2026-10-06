using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Shared.Dtos.DtosImpl;

public class SaveChickenActionFarmDto : IDto
{
    public int Id { get; set; }

    public string GenericName { get; set; } = string.Empty;

    public int FarmId { get; set; }

    [ValidateNever]
    public FarmDto? Farm { get; set; }

    public int SaveChickenActionId { get; set; }

    [ValidateNever]
    public SaveChickenActionDto? SaveChickenAction { get; set; }

    public int NumberOfChickensToBeSaved { get; set; } = 0;
    public int NumberOfRoostersToBeSaved { get; set; } = 0;
}
