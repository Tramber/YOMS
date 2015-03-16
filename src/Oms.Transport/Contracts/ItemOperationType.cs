using System.Runtime.Serialization;

namespace Oms.Transport.Contracts
{
     [DataContract]
    public enum ItemOperationType
    {
          [EnumMember]
        Add,
         [EnumMember]
        Remove,
         [EnumMember]
        Delete,
         [EnumMember]
        Update
    }
}