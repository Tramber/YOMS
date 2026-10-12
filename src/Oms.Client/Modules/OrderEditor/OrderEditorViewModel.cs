using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Oms.Client.Model;
using Oms.Client.Services;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Client.Modules.OrderEditor
{
    /// <summary>
    /// Order entry dialog. Save draft stashes the order, Send sends it for acceptance; the dialog closes when the server accepts it.
    /// Opened on an existing order, it edits that order and Update sends the change (the server decides whether the state allows it).
    /// </summary>
    public partial class OrderEditorViewModel : ObservableObject, IRequestClose
    {
        private readonly IOrderServiceProxy _proxy;
        private readonly Session _session;
        private OrderAdapter _edited;

        [ObservableProperty]
        private IReadOnlyList<SecurityDto> _securities = new SecurityDto[0];

        [ObservableProperty]
        private IReadOnlyList<FundDto> _funds = new FundDto[0];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand), nameof(UpdateCommand))]
        private SecurityDto _selectedSecurity;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand), nameof(UpdateCommand))]
        private FundDto _selectedFund;

        [ObservableProperty]
        private OrderDtoWay _selectedWay = OrderDtoWay.Buy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLimit))]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand), nameof(UpdateCommand))]
        private OrderDtoType _selectedOrderType = OrderDtoType.Limit;

        [ObservableProperty]
        private OrderDtoValidityType _selectedValidity = OrderDtoValidityType.Day;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand), nameof(UpdateCommand))]
        private decimal? _quantity;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand), nameof(UpdateCommand))]
        private decimal? _priceLimit;

        [ObservableProperty]
        private string _clientOrderRef;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand), nameof(UpdateCommand))]
        private bool _isBusy;

        public OrderEditorViewModel(IOrderServiceProxy proxy, Session session)
        {
            _proxy = proxy;
            _session = session;
        }

        public IReadOnlyList<OrderDtoWay> Ways { get; } = [OrderDtoWay.Buy, OrderDtoWay.Sell];

        public IReadOnlyList<OrderDtoType> OrderTypes { get; } = [OrderDtoType.Market, OrderDtoType.Limit];

        public IReadOnlyList<OrderDtoValidityType> Validities { get; } = [OrderDtoValidityType.Day, OrderDtoValidityType.EndOfDay, OrderDtoValidityType.Gtc
        ];

        /// <summary>A limit order needs a price.</summary>
        public bool IsLimit => SelectedOrderType == OrderDtoType.Limit;

        /// <summary>True when the dialog edits an existing order.</summary>
        public bool IsEditing => _edited != null;

        public bool IsNew => _edited == null;

        public string Title => IsEditing ? "Edit order " + _edited.Id : "New order";

        public event EventHandler CloseRequested;

        /// <summary>
        /// Loads the securities and funds the order can be placed on and selects the first of each, or, with an order to edit,
        /// the values of that order. Throws when the server cannot be reached.
        /// </summary>
        public async Task LoadAsync(OrderAdapter order = null)
        {
            Securities = await _proxy.GetSecuritiesAsync();
            Funds = await _proxy.GetFundsAsync();
            SelectedSecurity = Securities.Count > 0 ? Securities[0] : null;
            SelectedFund = Funds.Count > 0 ? Funds[0] : null;
            if (order == null) return;

            _edited = order;
            SelectedSecurity = Securities.FirstOrDefault(s => order.Security != null && s.Id == order.Security.Id) ?? SelectedSecurity;
            SelectedFund = Funds.FirstOrDefault(f => order.Fund != null && f.Id == order.Fund.Id) ?? SelectedFund;
            SelectedWay = order.Way;
            SelectedOrderType = order.OrderType;
            SelectedValidity = order.Validity;
            Quantity = (decimal)order.Quantity;
            PriceLimit = order.OrderType == OrderDtoType.Limit ? (decimal?)order.Price : null;
            ClientOrderRef = order.ClientOrderRef;
            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(IsNew));
            OnPropertyChanged(nameof(Title));
        }

        [RelayCommand]
        private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private Task StashAsync() => SubmitAsync(EditionCommand.Stash);

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private Task SendAsync() => SubmitAsync(EditionCommand.Create);

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private Task UpdateAsync() => SubmitAsync(EditionCommand.Update);

        private bool CanSubmit()
        {
            return !IsBusy
                && SelectedSecurity != null
                && SelectedFund != null
                && Quantity.HasValue && Quantity.Value > 0
                && (!IsLimit || (PriceLimit.HasValue && PriceLimit.Value > 0));
        }

        private async Task SubmitAsync(EditionCommand command)
        {
            ErrorMessage = null;
            IsBusy = true;
            try
            {
                var response = await _proxy.SendAsync(new OrderEditionRequest(_session.Login, command, [ToDto()], default(TransactionMode)));
                var result = response.ResultList[0].Item2;
                if (result.ErrorCode != 0)
                    ErrorMessage = result.ErrorMessage;
                else
                    Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private OrderDto ToDto()
        {
            return new OrderDto
            {
                Id = _edited == null ? 0 : _edited.Id,
                Owner = _edited == null ? null : _edited.Owner,
                Security = SelectedSecurity,
                Fund = SelectedFund,
                Way = SelectedWay,
                OrderType = SelectedOrderType,
                Validity = SelectedValidity,
                Quantity = (double)Quantity.Value,
                PriceLimit = IsLimit ? (double?)PriceLimit.Value : null,
                ClientOrderRef = ClientOrderRef,
            };
        }
    }
}
