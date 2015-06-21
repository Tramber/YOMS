using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.Funds
{
    public class Fund : IFund
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Portfolio { get; set; }
        public string Depositary { get; set; }
    }
}
