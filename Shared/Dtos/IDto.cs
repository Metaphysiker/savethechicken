using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Dtos
{
    public interface IDto : IEntityWithId
    {
        public string GenericName { get; set; }
    }
}
