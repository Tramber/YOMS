using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Transport.Contracts
{
    [ServiceContract(CallbackContract = typeof(IOrderServiceContractCallback), SessionMode = SessionMode.Required)]
    public interface IOrderServiceContract
    {
        [OperationContract]
        EditionResponse<OrderDto> Handle(OrderEditionRequest request);

        [OperationContract]
        Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderRequest request);

        [OperationContract]
        Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderMarketRequest request);

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

