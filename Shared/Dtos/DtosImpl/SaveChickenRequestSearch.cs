using Shared.Dtos.DtosImpl;

public class SaveChickenRequestSearch : ISearchDto
    {
        public List<int>? SaveChickenActionIds { get; set; }
        public int? PersonId { get; set; }

    }
