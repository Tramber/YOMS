using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using Oms.Server.Domain.Users;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            DiscriminateSubClassesOnColumn("Role");
            Table("t_user");
            Id(x => x.Id);
            Map(x => x.LastName);
            Map(x => x.Login);
            Map(x => x.FirstName);
        }
    }
}
