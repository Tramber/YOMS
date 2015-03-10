using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public enum OrderDtoValidityType
    {
        [EnumMember]
        Day,

        [EnumMember]
        EndOfDay,

        [EnumMember]
        Gtc,

        [EnumMember]
        Gtd,
    }
}