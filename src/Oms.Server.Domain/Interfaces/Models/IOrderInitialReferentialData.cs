using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrderInitialReferentialData
    {
        string InstrumentCode { get; }

        SecurityCodeType SecurityCodeType { get; }

        string FundCode { get; }

        FundCodeType FundCodeType { get; }

        string FundFolioCode { get; }
    }
}