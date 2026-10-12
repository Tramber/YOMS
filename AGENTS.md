# AGENTS.md

Guidance for AI coding agents working in this repository. See `README.md` for the project layout.

## Commands

- Build: `dotnet build src/Oms.sln`
- Test: `dotnet test src/Oms.sln`
- Run server: `dotnet run --project src/Oms.Server.host`

## Conventions

- .NET 10, SDK-style projects. Shared settings live in `src/Directory.Build.props`; `ImplicitUsings` and `Nullable` are off, so write explicit `using`s.
- `GenerateAssemblyInfo` is off: assembly attributes stay in each project's `Properties/AssemblyInfo.cs`.
- Packages are referenced per project (no `packages.config`, no central package management).
- Layering: `Domain` <- `DataAccess` / `Core` <- `host`. `Transport` holds DTOs shared by server and client. Keep domain code free of transport and persistence types.
- Transport is SignalR (`OrderHub`). Hub methods cannot be overloaded, so each request type has its own method name; shared names are in `OrderHubMethods`.
- `Oms.Server.Domain/Workflow/OrderStateMachine.cs` is generated from `OrderStateMachine.stateless`: edit the designer file, not the C#. Custom logic goes in `OrderStateMachine.Custom.cs`.

## MCP servers

`.mcp.json` registers two documentation servers (remote, no local process) so agents look up current APIs instead of guessing:

- `context7`: library docs (Avalonia, CommunityToolkit.Mvvm, NHibernate, Stateless, FluentValidation, SignalR, ...)
- `microsoft-learn`: official .NET and ASP.NET Core documentation

## Known issues

- Persistence: NHibernate maps the domain through `NHibernateHelper` (automapping). Interface-typed associations are pointed to their entity class by `InterfaceReferenceConvention`; the generic event logs (`OrderParameterEventLog<IOrderEditableData>`, ...) are added by `ExplicitTypeSource` because assembly scanning cannot find closed generics. The SQLite schema is created once, when the database file is missing or empty (never dropped or migrated), so delete `oms.sqlite` after changing a mapping.
- Requests identify the user by login (`Request.Token`); there is no authentication yet. A security with an `Undefined` currency or type cannot be mapped to a DTO, so an order on such a security fails after it was saved.
