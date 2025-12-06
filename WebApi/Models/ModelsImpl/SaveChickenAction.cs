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
        public string Title { get; set; } = String.Empty;
        [Required]
        public String Description { get; set; } = String.Empty;
        public List<SaveChickenRequest> SaveChickenRequests { get; set; } = new List<SaveChickenRequest>();
    }
}
