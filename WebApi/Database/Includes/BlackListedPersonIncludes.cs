using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class BlackListedPersonIncludes
    {
        public static readonly Expression<Func<BlackListedPerson, object?>>[] Default = new Expression<Func<BlackListedPerson, object?>>[]
        {
            r => r.Contact,
            r => r.Address
        };
    }
}
