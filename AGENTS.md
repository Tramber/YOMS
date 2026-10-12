# AGENTS.md

Guidance for AI coding agents working in this repository. See `README.md` for the project layout.

## Commands

- Build: `dotnet build src/Oms.sln`
- Test: `dotnet test src/Oms.sln`
- Run server: `dotnet run --project src/Oms.Server.host` (listens on `http://localhost:9001`)
- Run client: `dotnet run --project src/Oms.Client` (login `trader`, then Connect). Stop the server when you are done: a server left running keeps port 9001.

The server writes `oms.sqlite` in its working directory and seeds demo data on an empty database (`SeedDevelopmentData=false` turns it off). After changing a persistence mapping, delete the file: the schema is only created for a new or empty file.

## Conventions

- .NET 10, SDK-style projects. Shared settings live in `src/Directory.Build.props`; `ImplicitUsings` and `Nullable` are off, so write explicit `using`s.
- `GenerateAssemblyInfo` is off: assembly attributes stay in each project's `Properties/AssemblyInfo.cs`.
- Packages are referenced per project (no `packages.config`, no central package management).
- Layering: `Domain` <- `DataAccess` / `Core` <- `host`. `Transport` holds DTOs shared by server and client. Keep domain code free of transport and persistence types.
- Transport is SignalR (`OrderHub`). Hub methods cannot be overloaded, so each request type has its own method name; shared names are in `OrderHubMethods`. Messages go through System.Text.Json: a constructor parameter must match the name of a property, and a DTO property needs a public setter (`MessageSerializationTests` checks the round trip).
- Client view models talk to the server only through `IOrderServiceProxy` and report problems with `INotificationService`; keep them free of SignalR types so they can be tested with a fake.
- `Oms.Server.Domain/Workflow/OrderStateMachine.cs` is generated from `OrderStateMachine.stateless`: edit the designer file, not the C#. Custom logic goes in `OrderStateMachine.Custom.cs`.

## MCP servers

`.mcp.json` registers two documentation servers (remote, no local process) so agents look up current APIs instead of guessing:

- `context7`: library docs (Avalonia, CommunityToolkit.Mvvm, NHibernate, Stateless, FluentValidation, SignalR, ...)
- `microsoft-learn`: official .NET and ASP.NET Core documentation

## Known issues

- Persistence: NHibernate maps the domain through `NHibernateHelper` (automapping). Interface-typed associations are pointed to their entity class by `InterfaceReferenceConvention`; the generic event logs (`OrderParameterEventLog<IOrderEditableData>`, ...) are added by `ExplicitTypeSource` because assembly scanning cannot find closed generics. The SQLite schema is created once, when the database file is missing or empty (never dropped or migrated), so delete `oms.sqlite` after changing a mapping. A fingerprint of the generated DDL is stored in the file (`PRAGMA user_version`); a file from another mapping version makes the host fail at start with a message saying so. A `PostgreSQL` provider is also supported (`SchemaUpdate` adds missing tables); `OMS_TEST_DB=postgres dotnet test Oms.Server.DataAccess.Tests` runs the persistence tests against a Testcontainers PostgreSQL (needs Docker).
- Requests identify the user by login (`Request.Token`); there is no authentication yet.
- Trades work in the domain (`AddTrade`, `CancelTrade`, the booking steps, `TradeBooked`) but no hub command reaches them, so a client cannot book executions yet. `OrderStateCommand` only has Cancel, AcceptPending, StartBooking, RejectPending, Recall and Delete.
- The SignalR layer itself has no automated test: the hub's method names are not checked against the `OrderHubMethods` constants the client uses, and no test runs the client's `OrderServiceProxy` against the running host. The service, the messages and the persistence below it are tested.
- The client's Update of an existing order is not offered in the UI, although the server supports it.
