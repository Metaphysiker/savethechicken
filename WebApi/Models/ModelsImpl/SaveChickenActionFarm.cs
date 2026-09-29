namespace WebApi.Models.ModelsImpl;

public class SaveChickenActionFarm: IModel
{
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int FarmId { get; set; }
        public Farm Farm { get; set; } = null!;

        // Foreign Key to SaveChickenAction
        public int SaveChickenActionId { get; set; }
        public SaveChickenAction SaveChickenAction { get; set; } = null!;

        // Additional information on the relationship
        public int NumberOfChickensToBeSaved { get; set; } = 0;
        public int NumberOfRoostersToBeSaved { get; set; } = 0;
}
