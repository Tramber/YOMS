using System.Runtime.Serialization.Formatters;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Interfaces.Repository
{
    public interface IFundRepository : IRepository<IFund>
    {
    }
}