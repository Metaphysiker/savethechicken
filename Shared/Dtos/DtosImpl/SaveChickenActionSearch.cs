using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.DtosImpl
{
    public class SaveChickenActionSearch : ISearchDto
    {
        public bool? IsActive { get; set; }
    }
}
