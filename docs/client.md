# WPF client (`Oms.Client`)

`net10.0-windows` WPF application using Caliburn.Micro (MVVM), MEF for composition, MahApps.Metro for windows, Xceed AvalonDock for docking and Rx for async UI plumbing. It builds on any OS (`EnableWindowsTargeting`) but only runs on Windows.

> **State of the client:** it is a UI prototype. The order blotter shows 50 randomly generated orders and the order editor searches a hard-coded list of phrases. Nothing in the main UI talks to the server yet. `OrderServiceProxy` and `MainWindow` contain SignalR code but are not used by the startup path (see [Server connection](#server-connection)). The client compiles after the .NET 10 migration but has not been run.

## Start-up

1. `App.xaml` has no `StartupUri`. It instantiates `MefBootstrapper` as a resource and merges the dark AvalonDock and VS2013 theme dictionaries plus `Styles/Converters.xaml`.
2. `MefBootstrapper` (a Caliburn `BootstrapperBase`) builds a MEF `CompositionContainer` over every assembly in `AssemblySource`, and registers `MetroWindowManager` as `IWindowManager` and a single `EventAggregator`.
3. `OnStartup` calls `DisplayRootViewForAsync<IShell>()`; Caliburn resolves `ShellViewModel` through MEF and finds `ShellView` by naming convention.

View models are exported with `[Export(typeof(IXxx))]` and take dependencies with `[ImportingConstructor]`. `GetInstance`/`GetAllInstances` map Caliburn service lookups onto MEF contract names.

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
    OrderServiceProxy.cs   SignalR client (not wired in yet)
  Model/OrderAdapter.cs    bindable wrapper used by the blotter
  Framework/          reusable WPF/Caliburn helpers (below)
  Styles/             Converters.xaml, AvalonDock and VS2013 theme dictionaries
  MainWindow.xaml(.cs)     standalone SignalR test window (not shown at start-up)
```

## Modules

### Shell
`ShellViewModel` is a `Conductor<IScreen>.Collection.OneActive`. On initialisation it adds an `OrderBlotterViewModel` as a document and activates it, and subscribes to `INotification` messages via `IEventAggregator.SubscribeOnPublishedThread`. `ShellView` hosts an AvalonDock `DockingManager` whose `DocumentsSource` is the conductor's `Items` (each item rendered with `cal:View.Model`), two test buttons (`CreateOrder`, `CreateNotification`) and a `ListBox` overlay for notifications.

- `CreateOrder` opens `OrderEditorViewModel` as a dialog through `IWindowManager.ShowDialogAsync`.
- `CreateNotification` publishes an info notification.
- `Tools` returns `null`: there are no anchorable tool panes yet.

### Order blotter
`OrderBlotterViewModel` exposes a `CollectionViewSource` (`OrderViewSource`) over `BindableCollectionEx<OrderAdapter>` with live filtering, live grouping on `GroupId` and live sorting on `Id`. `CreateOrders()` fills it with random data.

- **Grouping:** `GroupOrders` assigns the next free group id (starting at 4000) to selected ungrouped orders; `UnGroupOrders` resets them to `0`. `CanGroupOrders`/`CanUnGroupOrders` follow Caliburn's `Can` convention. The group header context menu ("Disband Group") sends an `ActionMessage` to `UnGroupOrders` with the selected items.
- **Filtering:** the `FilterText` box feeds `FastSearch<OrderAdapter>` (see below).
- **View:** `ListView` + `GridView` with Id, Price, Quantity and GroupId columns, `HeaderPersister` to keep the group header visible, `SortingBehavior` for column sorting.

### Order editor
`OrderEditorViewModel` builds a small Rx pipeline over `PropertyChanged` for `SearchText`: short input (under 3 characters) is throttled 500 ms, longer input 300 ms, then `DistinctUntilChanged`. Each term runs `DoSearch` on a background thread (`Observable.ToAsync`) and `TakeUntil(input)` cancels stale searches. Results reload `SearchResults`, which a `ComboBox` with `AutoCompleteBehavior` displays; a log list shows the emitted terms. `DoSearch` filters a hard-coded array, so this is a demo of the pattern, not an order entry form. The view is a MahApps `MetroWindow`.

### Notifications
`INotificationService.Process(title, description, type)` publishes a `Notification` (random 64-bit id, timestamp, `NotificationType` Info/Warn/Error) on the event aggregator; `ProcessInfo/Warn/Error` extension methods wrap it. `ShellViewModel.HandleAsync` wraps each message in a `NotificationViewModel` and adds it to the overlay list. Notifications are never removed.

## Framework helpers

| Area | Types | Purpose |
|---|---|---|
| `Caliburn/` | `MetroWindowManager` | `WindowManager` that hosts views in a MahApps `MetroWindow`, centred on its owner |
| | `ObservableCommand`, `ObservableProperty`, `ObservableEvents` | Rx bridges: a command exposed as `IObservable<object>` with an observable `canExecute`; property and event streams |
| | `ShowShellCoroutine` | Caliburn `IResult` stub that does not do anything yet |
| `Observables/` | `BindableCollectionEx<T>` | `BindableCollection` with `Refresh` (re-raise a single reset after replacing items in place) and `Reload` |
| | `FastSearch<T>` | Builds compiled `Predicate<T>` filters from text such as `IsinCode = "FR0000"`, resolving nested property paths with expression trees |
| `Behaviors/` | `MultiSelectBehavior`, `SortingBehavior` (+ `Sorting`, `SortingAdorner`), `AutoCompleteBehavior` | `Microsoft.Xaml.Behaviors` behaviours for bindable multi-selection, header click sorting with an adorner, and editable combo box suggestions |
| `Controls/` | `HeaderPersister` (+ `HeaderAdorner`), `DynamicStyle`, `ExpanderEx`, `MyGridRowPresenter`, `VisualTreeHelpers`, `Filters/FilterExtension` | Sticky group headers, style switching, small control and visual-tree utilities, a filter markup extension |
| `Converters/` | `CollectionToVisibilityConverter`, `ModelSummaryConverter`, `DebugConverter` | Binding converters |
| `Selectors/` | `ItemGroupStyleSelector`, `SimpleDataTemplateSelector`, `OrderAdapterListTemplateSelector` | Group style and template selection (used by the blotter and AvalonDock) |
| `ShaderEffects/` | `GrayscaleEffect` (+ base and utility), `GrayscaleEffect.ps` | Pixel shader effect |

`Themes/` (`BlueTheme`, `DarkTheme`, `LightTheme`, `ThemeManager`) are MEF-exported theme classes that complement the XAML dictionaries in `Styles/Themes`.

## Server connection

The server is a SignalR hub at `http://localhost:9001/orders` (see [architecture.md](architecture.md)); method and event names are the constants in `OrderHubMethods` (`Oms.Transport`).

- `Services/OrderServiceProxy` wraps a `HubConnection` with automatic reconnect, `StartAsync()` (connect + `SubscribeToEvents`) and is an `IObservable<Tuple<ItemOperationType, OrderDto[]>>` fed by `OrderListUpdated`. Nothing instantiates it.
- `MainWindow` is a bare window with Subscribe/Unsubscribe buttons that does the same directly and appends received orders to an `Orders` collection (bound to an `ItemsControl` without a template). It is never shown, since startup goes through the bootstrapper.

To connect the real UI, the usual path is: register `OrderServiceProxy` as an exported service, inject it into `OrderBlotterViewModel`, map incoming `OrderDto`s to `OrderAdapter`s (`OrderAdapter` already mirrors the DTO fields) and replace `CreateOrders()`. The hub's `Handle*` methods return `null` today, so request/response calls need the server work noted in `AGENTS.md` first.

## Porting notes (.NET Framework 4.5 to .NET 10)

- Caliburn.Micro 2 to 4: `OnInitialize` is now `OnInitializeAsync`, `IHandle<T>.Handle` is `HandleAsync`, `Subscribe` is `SubscribeOnPublishedThread`, `TryClose`, `ShowDialog` and `Publish` became their `...Async` / `...OnUIThreadAsync` forms. The coroutine/`IResult` API in `ShowShellCoroutine` was not ported and the class is unused.
- MahApps.Metro 1.1 to 2.4: the `TitleCaps` property is now `TitleCharacterCasing`.
- Blend `System.Windows.Interactivity` to `Microsoft.Xaml.Behaviors` (XAML namespace `http://schemas.microsoft.com/xaml/behaviors`).
- MEF comes from the `System.ComponentModel.Composition` package.
- The Xceed AvalonDock themes still reference the old Extended WPF Toolkit; if theme loading fails at run time, check the AvalonDock theme assembly names in `Styles/Themes/AvalonDock`.
