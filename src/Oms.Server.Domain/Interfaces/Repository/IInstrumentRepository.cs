using System.Collections.Generic;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.Domain.Interfaces.Repository
{
    public interface IInstrumentRepository : IRepository<Security>
    {
        Security GetById(int id);
        IList<Security> GetAssetByKind(SecurityType securityType);
        IList<Security> GetAssetList();
    }
}