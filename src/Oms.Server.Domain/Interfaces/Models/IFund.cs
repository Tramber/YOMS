using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IFund : IIdentifiable
    {
        string Name { get; set; }
        string Portfolio { get; set; }
        string Depositary { get; set; }
    }
}