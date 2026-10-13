```text
██╗     ██╗  ████╗  ██╗     ██╗  ██████╗
╚═██╗ ██╔═╝██╔═══██╗████╗ ████║██╔═════╝
  ╚═██╔═╝  ██║   ██║██╔═██╔═██║╚═████╗
    ██║    ██║   ██║██║ ╚═╝ ██║  ╚═══██╗
    ██║    ╚═████╔═╝██║     ██║██████╔═╝
    ╚═╝      ╚═══╝  ╚═╝     ╚═╝╚═════╝
    Yet Another Order Management System
```

# YOMS

Order Management System: a .NET 10 server (domain, NHibernate/SQLite persistence, SignalR transport) and a cross-platform Avalonia client. Traders create orders, the server takes each order through its workflow (draft, acceptance, working, dealing, validation, booking), keeps the full history of every order, and pushes the changes to every connected client.

> **Legacy project.**
>
> YOMS is very old (around 2013): it was written for .NET Framework 4.5 (WCF, Unity, NUnit 2, `packages.config`) was made in *few days* and left untouched since then, it was a prototype. The original solution is in **DDD**, **Hexagonal**, **Event Sourcing**, **Actor Like (no framework, mailbox, thread affinity)**, **Aggregate Root are explicit FSM**. The GUI was in WPF and reactive. The functional domain is *fund management*, nothing complicated.
>
> The .NET 10 migration was a quick update to make it compile and run again, then the domain, the persistence and the client were completed on top of it. Expect rough edges: there is no authentication, the database is not migrated and there is no real booking system. The known problems are tracked in the [GitHub issues](https://github.com/Tramber/YOMS/issues).

## Getting started

You need the .NET 10 SDK (`global.json` pins it).

```
dotnet build src/Oms.sln
dotnet run --project src/Oms.Server.host     # the server, http://localhost:9001
dotnet run --project src/Oms.Client         # the client, in another terminal
```

In the client, keep the login `trader` and press **Connect**, then **Create Order**. The order blotter follows the orders of the server, and its toolbar acts on the selected ones (the server refuses what the state of an order does not allow). The first start of the server creates the SQLite database `oms.sqlite` in the working directory and fills it with demo data (the user `trader`, two funds, three securities). Delete the file to start again; set `SeedDevelopmentData=false` to start with an empty database, and `OMS_SERVER` on the client to use another server address. See [`docs/client.md`](docs/client.md) for what the client does.

## Layout (`src/`)

| Project | Role |
|---|---|
| `Oms.Server.Domain` | Orders, trades, validation (FluentValidation) and the Stateless order workflow |
| `Oms.Server.DataAccess` | NHibernate/FluentNHibernate repositories on SQLite |
| `Oms.Server.Core` | Application services, DTO mapping and the demo data |
| `Oms.Server.host` | ASP.NET Core host exposing the SignalR `OrderHub` at `http://localhost:9001/orders` |
| `Oms.Transport` | DTOs, messages and the shared service contract |
| `Oms.Client` | Avalonia desktop client (CommunityToolkit.Mvvm, Microsoft DI); runs on Windows, macOS and Linux |
| `Oms.Framework` | Shared utilities (task dispatchers) |

Each of these has a `*.Tests` project (NUnit) except `Oms.Transport`, which is covered by the tests of the projects that use it. `Oms.Server.host.Tests` runs the client's `OrderServiceProxy` against the host, in memory and on a real loopback port. `Oms.Server.DataAccess.Tests` runs the repositories and the order service against a real SQLite file.

## Documentation

- [`docs/architecture.md`](docs/architecture.md): components, request flow, the order model (event sourcing), the order and trade workflows, persistence, hosting and configuration.
- [`docs/client.md`](docs/client.md): using the client, its modules and its link to the server.
- `AGENTS.md` (also read by `CLAUDE.md`): commands, conventions and the format of GitHub issues, for people and coding agents. Known problems, missing features and technical debt are tracked in the [GitHub issues](https://github.com/Tramber/YOMS/issues).

## Build and test

```
dotnet build src/Oms.sln
dotnet test src/Oms.sln
```

CI (`.github/workflows/ci.yml`) runs both on Ubuntu for every pull request. The persistence tests also run against a PostgreSQL container (Testcontainers, needs Docker) with `OMS_TEST_DB=postgres dotnet test src/Oms.Server.DataAccess.Tests`; CI does not do that yet.
