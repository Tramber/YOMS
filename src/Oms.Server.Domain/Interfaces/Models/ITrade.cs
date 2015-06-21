using Oms.Server.Domain.Framework;

namespace Oms.Server.Domain.Interfaces.Models
{

    public interface ITradeData : ITradeEditableData
    {
        
    }

    public interface ITrade : ITradeData
    {
        IOrder Order { get; set; }
        GenericResult Create(ITriggerContext context, ITradeEditableData  editableData);
        GenericResult Cancel(ITriggerContext context);
    }
}