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
        public List<SaveChickenRequest> SaveChickenRequests { get; set; }
        public bool IsActive { get; set; } = false;

        SaveChickenAction()
        {
            SaveChickenRequests = new List<SaveChickenRequest>();
        }

    }
}
