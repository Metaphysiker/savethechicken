using Shared.Dtos.DtosImpl;

public class SaveChickenRequestSearch : ISearchDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<int> Ids { get; set; } = new List<int>();
        public string? SortBy { get; set; }
        public bool? SortDescending { get; set; }
        public string? SearchTerm { get; set; }
        public List<int>? SaveChickenActionIds { get; set; }

    }
