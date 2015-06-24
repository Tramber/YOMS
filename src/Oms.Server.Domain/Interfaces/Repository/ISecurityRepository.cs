using System.Collections.Generic;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.Domain.Interfaces.Repository
{
    public interface ISecurityRepository : IRepository<Security>
    {
        Security GetById(int id);
        IList<Security> GetBySecurityType(SecurityType securityType);
        IList<Security> GetActiveList();
    }
}