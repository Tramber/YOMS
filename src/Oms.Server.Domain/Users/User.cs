using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;

namespace Oms.Server.Domain.Users
{
    public class User : IIdentifiable
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Login { get; set; }
        public bool IsActive { get; set; }
    }
}
