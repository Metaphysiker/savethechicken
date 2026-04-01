using Shared.Dtos.DtosImpl;

public class SaveChickenDriveRequestSearch : ISearchDto
    {
        public List<int>? SaveChickenActionIds { get; set; }
        public int? PersonId { get; set; }

    }
