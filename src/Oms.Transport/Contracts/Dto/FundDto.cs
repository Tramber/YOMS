using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public class FundDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Portfolio { get; set; }
        public string Depositary { get; set; }
    }
}