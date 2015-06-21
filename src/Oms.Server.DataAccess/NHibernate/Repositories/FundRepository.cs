using System.Linq;
using System.Text;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    internal class FundRepository : RepositoryBase<IFund>, IFundRepository
    {
    }
}
