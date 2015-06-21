using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.Domain.Models.Orders
{
    internal class OrderInitialReferentialData : IOrderInitialReferentialData
    {
        public string InstrumentCode { get; internal set; }
        public SecurityCodeType SecurityCodeType { get; internal set; }
        public string FundCode { get; internal set; }
        public FundCodeType FundCodeType { get; internal set; }
        public string FundFolioCode { get; internal set; }
    }
}