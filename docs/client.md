# WPF client (`Oms.Client`)

`net10.0-windows` WPF application using CommunityToolkit.Mvvm (MVVM source generators and messenger), `Microsoft.Extensions.DependencyInjection` for composition, MahApps.Metro for windows, Xceed AvalonDock for docking and Rx for async UI plumbing. It builds on any OS (`EnableWindowsTargeting`) but only runs on Windows.

> **State of the client:** it is a UI prototype. The order blotter shows 50 randomly generated orders and the order editor searches a hard-coded list of phrases. Nothing in the main UI talks to the server yet. `OrderServiceProxy` and `MainWindow` contain SignalR code but are not used by the startup path (see [Server connection](#server-connection)). The client compiles after the .NET 10 migration but has not been run.

## Start-up

1. `App.xaml` has no `StartupUri`. It merges the dark AvalonDock and VS2013 theme dictionaries and `Styles/Converters.xaml`, and declares the implicit `DataTemplate`s that map `OrderBlotterViewModel` and `NotificationViewModel` to their views.
2. `App.OnStartup` builds a `ServiceProvider` (`IMessenger` = `WeakReferenceMessenger.Default`, `INotificationService`, `IWindowService`, `ShellViewModel`), then creates a MahApps `MetroWindow` whose content is `ShellView` and whose `DataContext` is the resolved `ShellViewModel`.

View models are plain classes (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`) constructed by DI or with `new`. There are no interfaces for view models: views are chosen by `DataTemplate` (docked documents, notifications) or by `WindowService` (dialogs).

## Structure

```
Oms.Client/
  Modules/            one folder per screen: View.xaml + ViewModel + interface
    Shell/            main window content, docking host, notification overlay
    OrderBlotter/     grid of orders: grouping, filter, sort
    OrderEditor/      order entry dialog with search-as-you-type
    Notification/     toast-like item shown in the shell
  Services/
    Notifications/    INotificationService + INotification published on the event aggregator
    WindowService.cs  opens dialog windows for a view model (IWindowService, IRequestClose)
    OrderServiceProxy.cs   SignalR client (not wired in yet)
  Model/OrderAdapter.cs    bindable wrapper used by the blotter
  Framework/          reusable WPF helpers (below)
  Styles/             Converters.xaml, AvalonDock and VS2013 theme dictionaries
  MainWindow.xaml(.cs)     standalone SignalR test window (not shown at start-up)
```

## Modules

### Shell
`ShellViewModel` holds an `ObservableCollection<object> Documents` (initially one `OrderBlotterViewModel`), the `ActiveDocument`, and a `Notifications` collection. It registers as an `IRecipient<INotification>` on the messenger. `ShellView` hosts an AvalonDock `DockingManager` whose `DocumentsSource` is `Documents` (each item rendered through the implicit `DataTemplate` for its view model), two test buttons bound to `CreateOrderCommand` and `CreateNotificationCommand`, and a `ListBox` overlay for notifications.

- `CreateOrder` opens `OrderEditorViewModel` as a dialog through `IWindowService.ShowDialog`.
- `CreateNotification` publishes an info notification.
- There are no anchorable tool panes yet.

### Order blotter
`OrderBlotterViewModel` exposes a `CollectionViewSource` (`OrderViewSource`) over `BindableCollectionEx<OrderAdapter>` (an `ObservableCollection` with `Refresh`/`Reload`) with live filtering, live grouping on `GroupId` and live sorting on `Id`. `CreateOrders()` fills it with random data.

- **Grouping:** `GroupOrdersCommand` assigns the next free group id (starting at 4000) to selected ungrouped orders; `UnGroupOrdersCommand` resets them to `0`. `CanExecute` follows the same rules, but `RelayCommand` does not re-query on selection change, so the menu items are not greyed out reliably; the handlers re-check and do nothing when not allowed. The list context menu binds the `ListView`'s `SelectedItems`; the group header menu ("Disband Group") reaches the view model through the header `Tag` and passes the group's `Items`.
- **Filtering:** the `FilterText` box feeds `FastSearch<OrderAdapter>` (see below).
- **View:** `ListView` + `GridView` with Id, Price, Quantity and GroupId columns, `HeaderPersister` to keep the group header visible, `SortingBehavior` for column sorting.

### Order editor
`OrderEditorViewModel` builds a small Rx pipeline over `PropertyChanged` for `SearchText`: short input (under 3 characters) is throttled 500 ms, longer input 300 ms, then `DistinctUntilChanged`. Each term runs `DoSearch` on a background thread (`Observable.ToAsync`) and `TakeUntil(input)` cancels stale searches. Results are observed on the UI `SynchronizationContext` captured in the constructor (plain `ObservableCollection`s do not marshal threads, unlike Caliburn's `BindableCollection`) and reload `SearchResults`, which a `ComboBox` with `AutoCompleteBehavior` displays; a log list shows the emitted terms. `DoSearch` filters a hard-coded array, so this is a demo of the pattern, not an order entry form. The view is a MahApps `MetroWindow`.

### Notifications
`INotificationService.Process(title, description, type)` sends a `Notification` (random 64-bit id, timestamp, `NotificationType` Info/Warn/Error) through `IMessenger`; `ProcessInfo/Warn/Error` extension methods wrap it. `ShellViewModel.Receive` wraps each message in a `NotificationViewModel` and adds it to the overlay list. Notifications are never removed.

## Framework helpers

| Area | Types | Purpose |
|---|---|---|
| `Observables/` | `BindableCollectionEx<T>` | `ObservableCollection` with `Refresh` (raise a single reset) and `Reload` (replace all items with one reset) |
| | `FastSearch<T>` | Builds compiled `Predicate<T>` filters from text such as `IsinCode = "FR0000"`, resolving nested property paths with expression trees |
| `Behaviors/` | `MultiSelectBehavior`, `SortingBehavior` (+ `Sorting`, `SortingAdorner`), `AutoCompleteBehavior` | `Microsoft.Xaml.Behaviors` behaviours for bindable multi-selection, header click sorting with an adorner, and editable combo box suggestions |
| `Controls/` | `HeaderPersister` (+ `HeaderAdorner`), `DynamicStyle`, `ExpanderEx`, `MyGridRowPresenter`, `VisualTreeHelpers`, `Filters/FilterExtension` | Sticky group headers, style switching, small control and visual-tree utilities, a filter markup extension |
| `Converters/` | `CollectionToVisibilityConverter`, `ModelSummaryConverter`, `DebugConverter` | Binding converters |
| `Selectors/` | `ItemGroupStyleSelector`, `SimpleDataTemplateSelector`, `OrderAdapterListTemplateSelector` | Group style and template selection (used by the blotter and AvalonDock) |
| `ShaderEffects/` | `GrayscaleEffect` (+ base and utility), `GrayscaleEffect.ps` | Pixel shader effect |

`Themes/` (`BlueTheme`, `DarkTheme`, `LightTheme`, `ThemeManager`) are theme classes (not registered or used anywhere at the moment) that complement the XAML dictionaries in `Styles/Themes`.

## Server connection

The server is a SignalR hub at `http://localhost:9001/orders` (see [architecture.md](architecture.md)); method and event names are the constants in `OrderHubMethods` (`Oms.Transport`).

- `Services/OrderServiceProxy` wraps a `HubConnection` with automatic reconnect, `StartAsync()` (connect + `SubscribeToEvents`) and is an `IObservable<Tuple<ItemOperationType, OrderDto[]>>` fed by `OrderListUpdated`. Nothing instantiates it.
- `MainWindow` is a bare window with Subscribe/Unsubscribe buttons that does the same directly and appends received orders to an `Orders` collection (bound to an `ItemsControl` without a template). It is never shown, since startup goes through the bootstrapper.

To connect the real UI, the usual path is: register `OrderServiceProxy` as an exported service, inject it into `OrderBlotterViewModel`, map incoming `OrderDto`s to `OrderAdapter`s (`OrderAdapter` already mirrors the DTO fields) and replace `CreateOrders()`. The hub's `Handle*` methods return `null` today, so request/response calls need the server work noted in `AGENTS.md` first.

## Porting notes

- **.NET Framework 4.5 to .NET 10** (first migration step): MahApps.Metro 1.1 to 2.4 (`TitleCaps` is now `TitleCharacterCasing`); Blend `System.Windows.Interactivity` to `Microsoft.Xaml.Behaviors` (XAML namespace `http://schemas.microsoft.com/xaml/behaviors`).
- **Caliburn.Micro and MEF to CommunityToolkit.Mvvm and Microsoft DI** (this step): removed `MefBootstrapper`, `MetroWindowManager`, the `Framework/Caliburn` helpers (unused Rx command/property bridges and a no-op coroutine) and the one-implementation view-model interfaces. Name-based action binding (`x:Name` buttons, `cal:ActionMessage`) became `Command` bindings; `Conductor` became a collection plus an active item; `IEventAggregator` became `IMessenger`; `WindowManager` became `IWindowService`.
- The Xceed AvalonDock themes still reference the old Extended WPF Toolkit; if theme loading fails at run time, check the AvalonDock theme assembly names in `Styles/Themes/AvalonDock`.
