using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;

namespace Oms.Server.Domain.Users
{
    public class Fund : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Portfolio { get; set; }
        public string Depositary { get; set; }
    }
}
