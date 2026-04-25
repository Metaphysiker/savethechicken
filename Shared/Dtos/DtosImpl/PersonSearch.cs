using Shared.Dtos.DtosImpl;

namespace Shared.Dtos.DtosImpl
{
    public class PersonSearch : ISearchDto
    {
        public bool? IsBlacklisted { get; set; }
        public bool? IsDriver { get; set; }

        /// <summary>
        /// Filter persons who have at least one unhandled request (IsHandled = false)
        /// </summary>
        public bool? HasUnhandledRequests { get; set; }

        /// <summary>
        /// Filter persons who have at least one unarchived request (IsArchived = false)
        /// </summary>
        public bool? HasUnarchivedRequests { get; set; }
    }
}
