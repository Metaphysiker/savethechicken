using Shared.Dtos.DtosImpl;

namespace Shared.Dtos.DtosImpl
{
    public class PersonSearch : ISearchDto
    {
        public bool? IsBlacklisted { get; set; }
        public bool? IsDriver { get; set; }
    }
}
