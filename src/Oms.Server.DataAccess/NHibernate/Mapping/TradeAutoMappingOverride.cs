using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Trades;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    public class TradeAutoMappingOverride : IAutoMappingOverride<Trade>
    {
        public void Override(AutoMapping<Trade> mapping)
        {
            // The quantity, amount, state and side are stored with the trade, unlike the computed data of an order.
            mapping.HasMany<TradeEventLog>(x => x.EventLogs).AsBag().Cascade.AllDeleteOrphan();
        }
    }
}
