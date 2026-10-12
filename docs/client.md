# Desktop client (`Oms.Client`)

Cross-platform desktop UI (Windows, macOS, Linux) built with Avalonia 12, `Avalonia.Controls.DataGrid`, CommunityToolkit.Mvvm (source-generated MVVM and messenger) and `Microsoft.Extensions.DependencyInjection`. It replaced a WPF client (Caliburn.Micro, MEF, MahApps.Metro, AvalonDock) that only ran on Windows.

## Running it

Start the server first (`dotnet run --project src/Oms.Server.host`, see the [README](../README.md)), then:

```
dotnet run --project src/Oms.Client
```

The client looks for the server at `http://localhost:9001`; set the `OMS_SERVER` environment variable to use another address.

1. Type your **login** (the demo data has the user `trader`, which is the default) and press **Connect**. The login is sent with every request, as the server has no authentication. The status line at the bottom shows `Connected as trader`; a failure appears as an error notification.
2. The **order blotter** fills with the orders of the server and then follows the changes the server pushes, including the ones made by other clients.
3. **Create Order** (enabled once connected) opens the order editor. **Save draft** stashes the order, **Send** sends it for acceptance; the dialog closes when the server accepts it and shows the server's message when it refuses.
4. Select one or more orders in the blotter and use the toolbar buttons to act on them. The server decides what is allowed in the order's state; a refused order appears as an error notification (`Cancel: order 12`, with the reason) and nothing changes.

## Start-up

`Program.Main` starts Avalonia with the Inter font. `App.OnFrameworkInitializationCompleted` builds a `ServiceProvider` and opens a window whose content is `ShellView` and whose `DataContext` is the resolved `ShellViewModel`. Registered services: `IMessenger` (`WeakReferenceMessenger.Default`), `INotificationService`, `IWindowService`, `Session` (the login), `IOrderServiceProxy` (the SignalR client) and `ShellViewModel`.

`App.axaml` declares the theme (Fluent, dark, plus the DataGrid theme) and the implicit `DataTemplate`s that map `OrderBlotterViewModel` and `NotificationViewModel` to their views. Compiled bindings are on (`AvaloniaUseCompiledBindingsByDefault`), so every view sets `x:DataType`; DataGrid columns set it to `OrderAdapter` individually.

## Structure

```
Oms.Client/
  Program.cs, App.axaml(.cs)     entry point, DI, theme, view-model-to-view templates
  Modules/                       one folder per screen: View.axaml(.cs) + ViewModel
    Shell/                       login, connection, tab host, notification overlay; DocumentViewModel base
    OrderBlotter/                order grid: live orders, commands, local grouping, filter
    OrderEditor/                 order entry dialog
    Notification/                toast item
  Services/
    IOrderServiceProxy.cs        the server's order service as the client sees it
    OrderServiceProxy.cs         SignalR implementation (OrderHub)
    Session.cs                   the login sent with every request
    WindowService.cs             IWindowService (modal dialogs) and IRequestClose
    Notifications/               INotificationService + INotification sent over the messenger
  Model/OrderAdapter.cs          bindable order row, built from an OrderDto
  Framework/Observables/FastSearch.cs   text filter compiled to a predicate
```

## Modules

### Shell
`ShellViewModel` owns the connection. **Connect** calls `IOrderServiceProxy.ConnectAsync` (which also subscribes to the pushed changes), loads the blotter and sets `IsConnected`; the login box is disabled from then on. **Create Order** loads the securities and funds, then opens `OrderEditorViewModel` through `IWindowService.ShowDialogAsync`; if the lists cannot be loaded, an error notification is shown instead of the dialog. The view is a toolbar (login, **Connect**, **Create Order**), a `TabControl` over `Documents` (tab header from `DocumentViewModel.DisplayName`, content through the implicit templates), a bottom-right notification overlay and the status line.

### Order blotter
`OrderBlotterViewModel` exposes a `DataGridCollectionView` (`OrdersView`) over an `ObservableCollection<OrderAdapter>`, grouped by `GroupId` and sorted by `Id`. The columns are Id, Reference, Security, Way, Type, Quantity, Remaining, Price, State, Status and Group.

- **Content.** `LoadAsync` replaces the rows by `GetOrdersAsync()` (it keeps the local group of the orders that stay). The proxy's pushed changes go through `Apply` on the UI thread: Add and Update insert or refresh the row (the cells update in place), Delete and Remove drop it.
- **Edit** opens the order editor on the selected order (exactly one; several give an error notification). The editor then shows **Update** instead of Save draft and Send and sends `EditionCommand.Update` with the order's id and owner. The server makes the update pending (status `UpdatePending`, the values stay the same) until **Accept pending** or **Reject pending**; a refusal, for a state that does not allow updates, is shown in the editor.
- **Commands** act on the selected rows and send one request with their ids, with the login as token: **Recall**, **Delete**, **Cancel**, **Accept pending** and **Reject pending** (`OrderRequest`), **Start booking** (`OrderRequest`), **Send to market** and **Cancel at market** (`OrderMarketRequest`), and the trade buttons (`OrderTradeRequest`): **Execute** trades what remains of each selected order in one trade at its limit price (0 for an order without one), and **Book trades** books the trades that are not booked yet. There is no form to enter a quantity or an amount. The buttons are always enabled: the server refuses what the state does not allow. The accepted orders are not reported; they show up through the pushed changes. Each refused order becomes an error notification with the server's message, and a failed call (server unreachable) a single error notification.
- **Group / Ungroup** are local to the client: grouping assigns the next free group id (from 4000) to ungrouped rows, ungrouping resets them to 0. The handlers refuse invalid selections, so the buttons are not greyed out.
- **Filter** accepts `Property = "value"` (nested paths allowed, for example `Security.IsinCode = "FR0000000001"`) through `FastSearch<OrderAdapter>`. Incomplete or invalid text shows all rows, and only string properties can be compared.
- Sorting is by clicking the column headers.

### Order editor
`OrderEditorViewModel` is a form for a new order: security and fund (lists from the server), way (Buy or Sell), type (Market or Limit), quantity, limit price (only for a limit order), validity (Day, EndOfDay or Gtc) and an optional reference. **Save draft** and **Send** are enabled once a security, a fund and a positive quantity (and a positive price for a limit order) are set. They send an `OrderEditionRequest` (`Stash` or `Create`) and, on success, raise `CloseRequested`, which `WindowService` turns into `Window.Close()`. A refusal by the server (its message) or a failed call is shown in the dialog, which stays open. **Close** closes it without sending anything.

Stop orders, the Gtd validity and the settlement and expiry dates are not offered (#25), so editing an order cannot change them.

### Notifications
`INotificationService.Process(title, description, type)` sends a `Notification` (random 64-bit id, timestamp, `NotificationType` Info/Warn/Error) as `INotification` through `IMessenger`; `ProcessInfo/Warn/Error` extension methods wrap it. `ShellViewModel` receives it and adds a `NotificationViewModel` to the overlay. Notifications are never removed.

## Server connection

`IOrderServiceProxy` is the only thing the view models know about the server, which keeps them testable with a fake. `OrderServiceProxy` implements it over a SignalR `HubConnection` with automatic reconnect, to the hub at `/orders` (see [architecture.md](architecture.md)); method and event names are the constants in `OrderHubMethods` (`Oms.Transport`).

| Member | Hub call |
|---|---|
| `ConnectAsync()` | starts the connection, then `SubscribeToEvents` |
| `GetOrdersAsync()`, `GetSecuritiesAsync()`, `GetFundsAsync()` | `GetOrders`, `GetSecurities`, `GetFunds` |
| `SendAsync(OrderEditionRequest)` | `HandleOrderEdition`: one `(OrderDto, GenericResultDto)` per order |
| `SendAsync(OrderRequest)` | `HandleOrder`: one `(orderId, GenericResultDto)` per order |
| `SendAsync(OrderMarketRequest)` | `HandleOrderMarket`: one `(orderId, GenericResultDto)` per order |
| `SendAsync(OrderTradeRequest)` | `HandleOrderTrade`: one `(orderId, GenericResultDto)` per trade |
| `Subscribe(observer)` | the `OrderListUpdated(ItemOperationType, OrderDto[])` event |

A call throws when the server cannot be reached; the view models turn that into a notification or an error message. A result with `ErrorCode != 0` is a refusal, and `ErrorMessage` says why.

## What changed from the WPF client

| WPF client                                                                             | Avalonia client                                                                                 |
|----------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------|
| Caliburn.Micro (conventions, `Conductor`, `IEventAggregator`, `IWindowManager`)        | CommunityToolkit.Mvvm (`ObservableObject`, `[RelayCommand]`, `IMessenger`) and `IWindowService` |
| MEF composition, one interface per view model                                          | `Microsoft.Extensions.DependencyInjection`, concrete view models                                |
| `ListView` + `GridView` + `CollectionViewSource`                                       | `DataGrid` + `DataGridCollectionView`                                                           |
| AvalonDock docking                                                                     | `TabControl` (one document today; `Dock.Avalonia` would be the docking equivalent)              |
| MahApps.Metro windows and the VS2013 theme dictionaries                                | Fluent dark theme                                                                               |
| `x:Name` and `cal:ActionMessage` bindings, group header context menu ("Disband Group") | `Command` bindings, toolbar buttons                                                             |
| Random sample orders, a search-as-you-type demo                                        | Live orders from the server and a real order form                                               |

Dropped without replacement: `HeaderPersister` (sticky group headers), the custom list behaviours (multi-select, sorting adorner, auto-complete), `DynamicStyle`, the grayscale pixel shader, the `ThemeManager` classes and the `Observable*` Caliburn helpers (all unused).
