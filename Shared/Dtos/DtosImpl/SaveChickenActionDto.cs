using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.DtosImpl
{
    public class SaveChickenActionDto : IDto
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
        public List<SaveChickenRequestDto> SaveChickenRequests { get; set; } = new List<SaveChickenRequestDto>();
        public bool IsActive { get; set; } = false;
    }
}
