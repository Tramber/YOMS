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
        void CreateOrders(OrderDto[] orderDtoList);

        [OperationContract]
        void CancelOrders(int[] orderIdList);

        [OperationContract]
        void UpdateOrders(OrderDto[] orderDtoList);

        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void SubscribeToEvents();

        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void UnsubscribeToEvents();
    }

    public interface IOrderServiceContractCallback
    {
        [OperationContract]
        void OrderListUpdated(ItemOperationType operationType, OrderDto[] orderDtoList);
    }
}

