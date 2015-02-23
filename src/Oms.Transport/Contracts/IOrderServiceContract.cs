using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts
{
    [ServiceContract(CallbackContract = typeof(IOrderServiceContractCallback), SessionMode = SessionMode.Required)]
    public interface IOrderServiceContract
    {
        [OperationContract]
        OrderDto[] GetOrders();

        [OperationContract]
        void CreateOrder(OrderDto order);

        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void SubscribeOrders();

        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void UnsubcribeOrders();
    }

    public interface IOrderServiceContractCallback
    {
        [OperationContract]
        void Orders_OnNext(OrderDto orderDto);
    }
}
