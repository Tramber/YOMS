# YOMS

Order Management System: a .NET 10 server (domain, NHibernate/SQLite persistence, SignalR transport) and a WPF client.

## Layout (`src/`)

| Project | Role |
|---|---|
| `Oms.Server.Domain` | Orders, trades, validation (FluentValidation) and the Stateless order workflow |
| `Oms.Server.DataAccess` | NHibernate/FluentNHibernate repositories on SQLite |
| `Oms.Server.Core` | Application services and DTO mapping |
| `Oms.Server.host` | ASP.NET Core host exposing the SignalR `OrderHub` at `http://localhost:9001/orders` |
| `Oms.Transport` | DTOs, messages and the shared service contract |
| `Oms.Client` | WPF client (Caliburn.Micro, MahApps.Metro, MEF); Windows only |
| `Oms.Framework` | Shared utilities |

## Build and test

```
dotnet build src/Oms.sln
dotnet test src/Oms.sln
dotnet run --project src/Oms.Server.host
```

`Oms.Client` and `Oms.Client.Tests` target `net10.0-windows`: they compile on macOS/Linux (`EnableWindowsTargeting`) but only run on Windows.
