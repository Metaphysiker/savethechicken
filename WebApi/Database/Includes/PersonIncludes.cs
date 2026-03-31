using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class PersonIncludes
    {
        public static readonly Expression<Func<Person, object?>>[] Default = new Expression<Func<Person, object?>>[]
        {
            p => p.Contact,
            p => p.Address,
        };
    }
}
