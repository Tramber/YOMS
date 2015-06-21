using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IUser : IIdentifiable
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string Login { get; set; }
        bool IsActive { get; set; }
    }
}