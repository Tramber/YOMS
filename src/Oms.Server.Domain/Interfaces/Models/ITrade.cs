using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Interfaces.Models
{

    public interface ITradeData : ITradeEditableData
    {
        
    }

    public interface ITrade : ITradeData, IIdentifiable
    {
        IOrder Order { get; set; }
        GenericResult Create(ITriggerContext context, ITradeEditableData  editableData);
        GenericResult Cancel(ITriggerContext context);
    }
}