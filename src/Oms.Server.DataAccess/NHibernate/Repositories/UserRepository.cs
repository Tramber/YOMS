using System.Collections.Generic;
using System.Linq;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    internal class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public User GetUserById(int id)
        {
            return base.GetById(id);
        }

        public User GetUserByLogin(string login)
        {
            return base.GetByColumn("Login", login).FirstOrDefault();
        }
        public IList<User> GetUserList()
        {
            return base.GetByColumn("IsActive", true);
        }
    }
}
