using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using Oms.Server.Domain.Orders;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    public class OrderGroupMap : ClassMap<OrderGroup>
    {
        public OrderGroupMap()
        {
            Table("t_ordergroup");
            Id(x => x.Id).Column("ordergrpid");
            References(x => x.Creator).Column("user_creatorid");
            HasMany(x => x.Orders).Inverse().Cascade.All();
        }
    }
}
