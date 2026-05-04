using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using WebApi.Interfaces;
using WebApi.Models.ModelsImpl;

namespace WebApi.Models.ModelsImpl
{
    public class SaveChickenRequest : IModel, IEntityWithFiles
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int? PersonId { get; set; }
        public Person? Person { get; set; }

        public int NumberOfChickensToBeSaved { get; set; } = 0;
        public int NumberOfRoostersToBeSaved { get; set; } = 0;
        public int NumberOfBoxes { get; set; } = 0;
        public string DescriptionOfPlaceForChickens { get; set; } = string.Empty;
        public bool AcceptTermsAndConditions { get; set; } = false;
        public bool ConfirmThatIFulfillCriteria { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public int? SaveChickenActionId { get; set; }
        public SaveChickenAction? SaveChickenAction { get; set; }
        public string Color { get; set; } = string.Empty;
        public List<StoredFile> Files { get; set; }
        public List<int> BlackListedPersonIds { get; set; } = new List<int>();
        public bool IsSubmittedFromPublicForm { get; set; } = false;
        public bool IsHandled { get; set; } = false;
        [Column(TypeName = "tsvector")]
        public NpgsqlTsVector SearchVector { get; set; } = null!;

        public SaveChickenRequest()
        {
            Files = new List<StoredFile>();
        }
    }
}
