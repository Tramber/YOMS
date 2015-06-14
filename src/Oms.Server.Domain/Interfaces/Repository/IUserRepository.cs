using System.Collections.Generic;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Interfaces.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        User GetUserById(int id);
        IList<User> GetUserByLogin(string login);
        IList<User> GetUserList();
    }
}