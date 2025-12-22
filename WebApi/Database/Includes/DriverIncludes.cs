using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class DriverIncludes
    {
        public static readonly Expression<Func<Driver, object?>>[] Default = new Expression<Func<Driver, object?>>[]
        {
            r => r.Contact,
            r => r.Address,
            r => r.SaveChickenAction,
        };
    }
}
