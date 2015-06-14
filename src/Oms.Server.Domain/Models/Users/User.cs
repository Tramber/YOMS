using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Models.Users
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
