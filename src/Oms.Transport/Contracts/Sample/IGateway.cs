using System.ServiceModel;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts
{
    [ServiceContract]
    public interface IGateway
    {
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [FaultContract(typeof(BusinessLogicFault))]
        Response ExecuteRequest(Request request);
    }
}