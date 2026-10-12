using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Model
{
    /// <summary>Bindable order row of the blotter. <c>GroupId</c> 0 means not grouped; it is local to this client.</summary>
    public partial class OrderAdapter : ObservableObject
    {
        public int Id { get; set; }

        [ObservableProperty]
        private double _quantity;

        [ObservableProperty]
        private double _remainingQuantity;

        [ObservableProperty]
        private double _price;

        [ObservableProperty]
        private string _state;

        [ObservableProperty]
        private string _status;

        [ObservableProperty]
        private int _groupId;

        /// <summary>The executions of the order, cancelled ones included.</summary>
        public IReadOnlyList<TradeDto> Trades { get; private set; } = new List<TradeDto>();

        public string ClientOrderRef { get; set; }

        public OrderDtoWay Way { get; set; }

        public SecurityDto Security { get; set; }

        public FundDto Fund { get; set; }

        public UserDto Owner { get; set; }

        public UserDto Creator { get; set; }

        public string Version { get; set; }

        public DateTime CreationDate { get; set; }

        public OrderDtoType OrderType { get; set; }

        public OrderDtoValidityType Validity { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string SecurityName
        {
            get { return Security == null ? null : Security.Name; }
        }

        public static OrderAdapter From(OrderDto dto)
        {
            var adapter = new OrderAdapter { Id = dto.Id };
            adapter.UpdateFrom(dto);
            return adapter;
        }

        /// <summary>Takes the values the server sent; the local group is kept.</summary>
        public void UpdateFrom(OrderDto dto)
        {
            ClientOrderRef = dto.ClientOrderRef;
            Way = dto.Way;
            Security = dto.Security;
            Fund = dto.Fund;
            Owner = dto.Owner;
            Creator = dto.Creator;
            Version = dto.Version;
            CreationDate = dto.CreationDate;
            OrderType = dto.OrderType;
            Validity = dto.Validity;
            ExpiryDate = dto.ExpiryDate;
            Quantity = dto.Quantity;
            RemainingQuantity = dto.RemainingQuantity;
            Price = dto.PriceLimit ?? 0;
            State = dto.State;
            Status = dto.Status;
            Trades = dto.Trades ?? new List<TradeDto>();
            OnPropertyChanged(nameof(SecurityName));
        }
    }
}
