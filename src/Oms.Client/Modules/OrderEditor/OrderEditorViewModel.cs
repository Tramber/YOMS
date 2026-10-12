using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Oms.Client.Services;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Client.Modules.OrderEditor
{
    /// <summary>Order entry dialog. Save draft stashes the order, Send sends it for acceptance; the dialog closes when the server accepts it.</summary>
    public partial class OrderEditorViewModel : ObservableObject, IRequestClose
    {
        private readonly IOrderServiceProxy _proxy;
        private readonly Session _session;

        [ObservableProperty]
        private IReadOnlyList<SecurityDto> _securities = new SecurityDto[0];

        [ObservableProperty]
        private IReadOnlyList<FundDto> _funds = new FundDto[0];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand))]
        private SecurityDto _selectedSecurity;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand))]
        private FundDto _selectedFund;

        [ObservableProperty]
        private OrderDtoWay _selectedWay = OrderDtoWay.Buy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLimit))]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand))]
        private OrderDtoType _selectedOrderType = OrderDtoType.Limit;

        [ObservableProperty]
        private OrderDtoValidityType _selectedValidity = OrderDtoValidityType.Day;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand))]
        private decimal? _quantity;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand))]
        private decimal? _priceLimit;

        [ObservableProperty]
        private string _clientOrderRef;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StashCommand), nameof(SendCommand))]
        private bool _isBusy;

        public OrderEditorViewModel(IOrderServiceProxy proxy, Session session)
        {
            _proxy = proxy;
            _session = session;
        }

        public IReadOnlyList<OrderDtoWay> Ways { get; } = new[] { OrderDtoWay.Buy, OrderDtoWay.Sell };

        public IReadOnlyList<OrderDtoType> OrderTypes { get; } = new[] { OrderDtoType.Market, OrderDtoType.Limit };

        public IReadOnlyList<OrderDtoValidityType> Validities { get; } = new[] { OrderDtoValidityType.Day, OrderDtoValidityType.EndOfDay, OrderDtoValidityType.Gtc };

        /// <summary>A limit order needs a price.</summary>
        public bool IsLimit => SelectedOrderType == OrderDtoType.Limit;

        public event EventHandler CloseRequested;

        /// <summary>Loads the securities and funds the order can be placed on and selects the first of each. Throws when the server cannot be reached.</summary>
        public async Task LoadAsync()
        {
            Securities = await _proxy.GetSecuritiesAsync();
            Funds = await _proxy.GetFundsAsync();
            SelectedSecurity = Securities.Count > 0 ? Securities[0] : null;
            SelectedFund = Funds.Count > 0 ? Funds[0] : null;
        }

        [RelayCommand]
        private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private Task StashAsync() => SubmitAsync(EditionCommand.Stash);

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private Task SendAsync() => SubmitAsync(EditionCommand.Create);

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
                var response = await _proxy.SendAsync(new OrderEditionRequest(_session.Login, command, new List<OrderDto> { ToDto() }, default(TransactionMode)));
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
