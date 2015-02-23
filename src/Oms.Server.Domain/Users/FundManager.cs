using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;

namespace Oms.Server.Domain.Users
{
    public class FundManager : User, IIdentifiable
    {
        public Fund Fund { get; set; }
    }
}
