using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    internal class InstrumentRepository : RepositoryBase<Security>, IInstrumentRepository
    {
        public Security GetById(int id)
        {
            return base.GetById(id);
        }

        public IList<Security> GetAssetByKind(SecurityType securityType)
        {
            return base.GetByColumn("Type", securityType);
        }

        public IList<Security> GetAssetList()
        {
            return base.GetByColumn("IsActive", true);
        }
    }
}
