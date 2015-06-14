using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Instruments;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrderInitialReferentialData
    {
        string InstrumentCode { get; }

        InstrumentCodeType InstrumentCodeType { get; }

        string FundCode { get; }

        FundCodeType FundCodeType { get; }

        string FundFolioCode { get; }
    }
}