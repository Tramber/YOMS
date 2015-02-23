using System.Collections.Generic;
using Oms.Server.Domain.Users;

namespace Oms.Server.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User GetUserById(int id);
        IList<User> GetUserByLogin(string login);
        IList<User> GetUserList();
    }
}