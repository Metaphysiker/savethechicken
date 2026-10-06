using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class SaveChickenActionFarmIncludes
    {
        public static readonly Expression<Func<SaveChickenActionFarm, object?>>[] Default = new Expression<Func<SaveChickenActionFarm, object?>>[]
        {
            s => s.SaveChickenAction,
            s => s.Farm,
            s => s.Farm.Contact,
            s => s.Farm.Address,
        };
    }
}
