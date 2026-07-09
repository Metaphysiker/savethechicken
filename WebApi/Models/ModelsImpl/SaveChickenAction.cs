using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.ModelsImpl
{
    public class SaveChickenAction : IModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Required]
        public List<DateOnly> Dates { get; set; } = new List<DateOnly>();
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;

        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// JSON serialized drive plans from the route planner
        /// </summary>
        public string? RoutePlansJson { get; set; }

        public string? DriverRoutePlansJson { get; set; }

        public List<SaveChickenRequest> SaveChickenRequests { get; set; }
        public List<Farm> Farms { get; set; }
        public List<SaveChickenDriveRequest> SaveChickenDriveRequests { get; set; }

        public SaveChickenAction()
        {
            SaveChickenRequests = new List<SaveChickenRequest>();
            Farms = new List<Farm>();
            SaveChickenDriveRequests = new List<SaveChickenDriveRequest>();
        }

    }
}
