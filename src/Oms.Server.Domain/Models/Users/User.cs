using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.Users
{
    public class User : IUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Login { get; set; }
        public bool IsActive { get; set; }
    }
}
