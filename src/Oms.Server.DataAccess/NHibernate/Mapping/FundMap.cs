using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using Oms.Server.Domain.Users;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    public class FundMap : ClassMap<Fund>
    {
        public FundMap()
        {
            Table("t_fund");
            Id(x => x.Id).Column("fundid");
            Map(x => x.Name);
            Map(x => x.Portfolio);
        }
    }
}
