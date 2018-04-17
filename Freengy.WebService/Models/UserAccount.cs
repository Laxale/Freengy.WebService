using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freengy.WebService.Interfaces;

namespace Freengy.WebService.Models
{
    internal class UserAccount : INamedObject, IObjectWithId 
    {
        public Guid Id { set; get; }

        public string Name { get; set; }
    }
}