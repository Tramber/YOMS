using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Instruments;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    internal class InstrumentRepository : RepositoryBase<Instrument>, IInstrumentRepository
    {
        public Instrument GetAssetById(int id)
        {
            return base.GetById(id);
        }

        public IList<Instrument> GetAssetByKind(InstrumentType instrumentType)
        {
            return base.GetByColumn("Type", instrumentType);
        }

        public IList<Instrument> GetAssetList()
        {
            return base.GetByColumn("IsActive", true);
        }
    }
}
