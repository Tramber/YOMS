# Desktop client (`Oms.Client`)

Cross-platform desktop UI (Windows, macOS, Linux) built with Avalonia 12, `Avalonia.Controls.DataGrid`, CommunityToolkit.Mvvm (source-generated MVVM and messenger) and `Microsoft.Extensions.DependencyInjection`. It replaced a WPF client (Caliburn.Micro, MEF, MahApps.Metro, AvalonDock) that only ran on Windows.

```
dotnet run --project src/Oms.Client
```

> **State of the client:** a UI prototype. The order blotter shows 50 randomly generated orders and the order editor searches a hard-coded list of phrases. The only server link is the **Connect to server** button, and the server does not push orders yet (see `AGENTS.md`).

## Start-up

`Program.Main` starts Avalonia with the Inter font. `App.OnFrameworkInitializationCompleted` builds a `ServiceProvider` (`IMessenger` = `WeakReferenceMessenger.Default`, `INotificationService`, `IWindowService`, `ShellViewModel`) and opens a window whose content is `ShellView` and whose `DataContext` is the resolved `ShellViewModel`.

`App.axaml` declares the theme (Fluent, dark, plus the DataGrid theme) and the implicit `DataTemplate`s that map `OrderBlotterViewModel` and `NotificationViewModel` to their views. Compiled bindings are on (`AvaloniaUseCompiledBindingsByDefault`), so every view sets `x:DataType`; DataGrid columns set it to `OrderAdapter` individually.

## Structure

```
Oms.Client/
  Program.cs, App.axaml(.cs)     entry point, DI, theme, view-model-to-view templates
  Modules/                       one folder per screen: View.axaml(.cs) + ViewModel
    Shell/                       tab host, toolbar, notification overlay, connection status; DocumentViewModel base
    OrderBlotter/                order grid with grouping, sorting and filtering
    OrderEditor/                 order entry dialog with search-as-you-type
    Notification/                toast item
  Services/
    WindowService.cs             IWindowService (modal dialogs) and IRequestClose
    OrderServiceProxy.cs         SignalR client for the OrderHub
    Notifications/               INotificationService + INotification sent over the messenger
  Model/OrderAdapter.cs          bindable order row
  Framework/Observables/FastSearch.cs   text filter compiled to a predicate
```

## Modules

### Shell
`ShellViewModel` holds `Documents` (an `ObservableCollection<DocumentViewModel>`, initially one `OrderBlotterViewModel`), the `ActiveDocument`, `Notifications` and `ConnectionStatus`. `ShellView` shows a toolbar (**Create Order**, **Create Notification**, **Connect to server**), a `TabControl` over `Documents` (tab header from `DocumentViewModel.DisplayName`, content through the implicit templates), a bottom-right notification overlay and a status line.

- **Create Order** opens `OrderEditorViewModel` through `IWindowService.ShowDialogAsync`.
- **Create Notification** sends an info notification.
- **Connect to server** creates an `OrderServiceProxy`, subscribes to `OrderListUpdated` and starts the connection; incoming orders are posted to the UI thread and added to the blotter.

### Order blotter
`OrderBlotterViewModel` exposes a `DataGridCollectionView` (`OrdersView`) over an `ObservableCollection<OrderAdapter>`, grouped by `GroupId` and sorted by `Id`.

- **Group / Ungroup** buttons act on the grid's selected rows: grouping assigns the next free group id (from 4000) to ungrouped rows, ungrouping resets them to 0. The handlers refuse invalid selections, so the buttons are not greyed out.
- **Filter** accepts `Property = "value"` (nested paths allowed, for example `Security.IsinCode = "FR0000"`) through `FastSearch<OrderAdapter>`. Incomplete or invalid text shows all rows, and only string properties can be compared.
- Sorting is by clicking the column headers.

### Order editor
`OrderEditorViewModel` builds an Rx pipeline over changes to `SearchText`: short input (under 3 characters) is throttled 500 ms, longer input 300 ms, then `DistinctUntilChanged`. Each term is searched on a background thread (`Observable.ToAsync`) and `TakeUntil(input)` drops stale results; results and the log are observed on the UI `SynchronizationContext` captured in the constructor. The view is an `AutoCompleteBox` for the suggestions plus a log list; `DoSearch` filters a hard-coded array, so this demonstrates the pattern rather than real order entry. **Close** raises `CloseRequested`, which `WindowService` turns into `Window.Close()`.

### Notifications
`INotificationService.Process(title, description, type)` sends a `Notification` (random 64-bit id, timestamp, `NotificationType` Info/Warn/Error) as `INotification` through `IMessenger`; `ProcessInfo/Warn/Error` extension methods wrap it. `ShellViewModel` receives it and adds a `NotificationViewModel` to the overlay. Notifications are never removed.

## Server connection

The server is a SignalR hub at `http://localhost:9001/orders` (see [architecture.md](architecture.md)); method and event names are the constants in `OrderHubMethods` (`Oms.Transport`). `OrderServiceProxy` wraps a `HubConnection` with automatic reconnect: `StartAsync()` connects and calls `SubscribeToEvents`, and the proxy is an `IObservable<Tuple<ItemOperationType, OrderDto[]>>` fed by `OrderListUpdated`. The hub's `Handle*` methods return `null` today, so request/response calls need the server work noted in `AGENTS.md` first.

## What changed from the WPF client

| WPF client | Avalonia client |
|---|---|
| Caliburn.Micro (conventions, `Conductor`, `IEventAggregator`, `IWindowManager`) | CommunityToolkit.Mvvm (`ObservableObject`, `[RelayCommand]`, `IMessenger`) and `IWindowService` |
| MEF composition, one interface per view model | `Microsoft.Extensions.DependencyInjection`, concrete view models |
| `ListView` + `GridView` + `CollectionViewSource` | `DataGrid` + `DataGridCollectionView` |
| AvalonDock docking | `TabControl` (one document today; `Dock.Avalonia` would be the docking equivalent) |
| MahApps.Metro windows and the VS2013 theme dictionaries | Fluent dark theme |
| `x:Name` and `cal:ActionMessage` bindings, group header context menu ("Disband Group") | `Command` bindings, toolbar buttons |

Dropped without replacement: `HeaderPersister` (sticky group headers), the custom list behaviours (multi-select, sorting adorner, auto-complete), `DynamicStyle`, the grayscale pixel shader, the `ThemeManager` classes and the `Observable*` Caliburn helpers (all unused).
