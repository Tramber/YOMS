# YOMS

Order Management System: a .NET 10 server (domain, NHibernate/SQLite persistence, SignalR transport) and a WPF client.

> **Legacy project.** YOMS is very old: it was written for .NET Framework 4.5 (WCF, Unity, NUnit 2, `packages.config`) and left untouched for years. The .NET 10 migration was a quick update just to make it compile and start again, not a modernization. Expect rough edges: unfinished features (the service `Handle*` methods return `null`), 4 failing domain tests, and a WPF client that compiles but has not been run. See `AGENTS.md` for known issues.

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

See [`docs/architecture.md`](docs/architecture.md) for the server architecture, request flow and order workflow, and [`docs/client.md`](docs/client.md) for the WPF client.

## Build and test

```
dotnet build src/Oms.sln
dotnet test src/Oms.sln
dotnet run --project src/Oms.Server.host
```

`Oms.Client` and `Oms.Client.Tests` target `net10.0-windows`: they compile on macOS/Linux (`EnableWindowsTargeting`) but only run on Windows.
