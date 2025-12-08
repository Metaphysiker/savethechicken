using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Dtos.DtosImpl
{
    public class ISearchDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<int> Ids { get; set; } = new List<int>();
        public string? SortBy { get; set; }
        public bool? SortDescending { get; set; }
        public string? SearchTerm { get; set; }
    }
}
