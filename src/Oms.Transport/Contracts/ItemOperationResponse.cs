using System.Collections.Generic;

namespace Oms.Transport.Contracts
{
    public class ItemOperationResponse<T>
    {
        public bool IsSuccess { get; set; }
        public ItemOperationType OperationType { get; private set; }
        public List<T> ItemList { get; set; }

        public ItemOperationResponse(bool isSuccess, ItemOperationType operationType, List<T> itemList)
        {
            this.IsSuccess = isSuccess;
            OperationType = operationType;
            ItemList = itemList;
        }
    }
}