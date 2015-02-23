using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using Oms.Server.Domain.Assets;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    public class AssetMap : ClassMap<Asset>
    {
        public AssetMap()
        {
            Table("t_asset");
            Id(x => x.Id).Column("assetid");
            Map(x => x.BloombergTicker);
            Map(x => x.Currency);
            Map(x => x.IsinCode);
            Map(x => x.Type);
            Map(x => x.Name);
            Map(x => x.IsActive);
            Map(x => x.CreationDate);
            Map(x => x.Origin);
            References(x => x.Creator).Column(CreatorColumnName).Cascade.All();
        }

        public static readonly string CreatorColumnName = "user_creatorid";
    }
}
