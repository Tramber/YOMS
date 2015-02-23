using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using Oms.Server.Domain.Orders;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    class OrderMap : ClassMap<Order>
    {
        public OrderMap()
        {
            Table("t_order");
            Id(x => x.Id).Column("orderid");
            Map(x => x.Price);
            Map(x => x.Quantity);
            Map(x => x.Way);
            Map(x => x.IsActive);
            Map(x => x.CreationDate);
            Map(x => x.OrderType);
            Map(x => x.OrderValidity);
            Map(x => x.ExpiryDate);
            References(x => x.Creator).Column(CreatorColumnName).Cascade.All();
            References(x => x.Owner).Column(OwnerColumnName).Cascade.All();
            References(x => x.Asset).Column(AssetColumnName).Cascade.All();
            References(x => x.Group).Column(GroupColumnName);
        }

        public static readonly string CreatorColumnName = "user_creatorid";
        public static readonly string OwnerColumnName = "user_ownerid";
        public static readonly string AssetColumnName = "asset_assetid";
        public static readonly string GroupColumnName = "grporder_grporderid";
    }
}
