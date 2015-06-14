using System.Collections.Generic;
using Oms.Server.Domain.Models.Instruments;

namespace Oms.Server.Domain.Interfaces.Repository
{
    public interface IInstrumentRepository : IRepository<Instrument>
    {
        Instrument GetAssetById(int id);
        IList<Instrument> GetAssetByKind(InstrumentType instrumentType);
        IList<Instrument> GetAssetList();
    }
}