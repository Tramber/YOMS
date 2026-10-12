# Avalonia proof of concept (`Oms.Client.Avalonia`)

A cross-platform UI for the order blotter, built to check whether the WPF client (Windows only) can move to Avalonia and run on macOS and Linux. Avalonia 12, Fluent dark theme, `Avalonia.Controls.DataGrid`, CommunityToolkit.Mvvm.

```
dotnet run --project src/Oms.Client.Avalonia
```

## What it shows

| Feature | WPF client | Avalonia PoC |
|---|---|---|
| Order grid | `ListView` + `GridView`, `CollectionViewSource` | `DataGrid` over `DataGridCollectionView` (grouping, sorting, filtering) |
| Group by `GroupId`, sort by `Id` | `PropertyGroupDescription`, `SortDescription` | `DataGridPathGroupDescription`, `DataGridSortDescription` |
| Group / ungroup selected orders | context menu commands | toolbar buttons bound to `SelectedItems` |
| Filter | `FastSearch` expression parser | plain "contains" match on the row values |
| Notifications | overlay `ListBox` | list under the grid (same `NotificationService` and messenger) |
| Server link | not wired | **Connect to server** button: SignalR `OrderServiceProxy`, appends orders pushed by the hub |

## What is shared with the WPF client

Linked as source files (no WPF dependency in them): `OrderAdapter`, `OrderServiceProxy` and the `Services/Notifications` classes. Everything else, including the view model, is new, because the WPF `OrderBlotterViewModel` uses `System.Windows.Data.CollectionViewSource`. A real migration would move the shared code into a `net10.0` class library referenced by both clients.

## Not carried over

MahApps.Metro window chrome and themes, AvalonDock docking (`Dock.Avalonia` is the equivalent), the order editor dialog, `HeaderPersister`, the custom behaviours and the pixel shader.

## Status

Builds, and starts and stays up on macOS (checked for 8 seconds). The UI was not inspected (no screenshot access in the test environment) and the server connection was not exercised, so grouping, filtering and **Connect** are untested. The server only pushes orders once the `Handle*` work in `AGENTS.md` is done.
