using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Funds;

namespace Oms.Server.Domain.Models.Users
{
    public class FundManager : User, IIdentifiable
    {
        public Fund Fund { get; set; }
    }
}
