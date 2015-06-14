using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Models.Funds
{
    public class Fund : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Portfolio { get; set; }
        public string Depositary { get; set; }
    }
}
