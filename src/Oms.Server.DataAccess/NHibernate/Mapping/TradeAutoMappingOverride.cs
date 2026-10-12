using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Trades;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    public class TradeAutoMappingOverride : IAutoMappingOverride<Trade>
    {
        public void Override(AutoMapping<Trade> mapping)
        {
            foreach (var propertyInfo in typeof(ITradeEditableData).GetProperties())
            {
                ((IPropertyIgnorer)mapping).IgnoreProperty(propertyInfo.Name);
            }

            mapping.HasMany<TradeEventLog>(x => x.EventLogs).AsBag().Cascade.AllDeleteOrphan();
        }
    }
}
