# AGENTS.md

Guidance for AI coding agents working in this repository. See `README.md` for the project layout.

## Commands

- Build: `dotnet build src/Oms.sln`
- Test: `dotnet test src/Oms.sln` (`Oms.Client.Tests` only runs on Windows)
- Run server: `dotnet run --project src/Oms.Server.host`

## Conventions

- .NET 10, SDK-style projects. Shared settings live in `src/Directory.Build.props`; `ImplicitUsings` and `Nullable` are off, so write explicit `using`s.
- `GenerateAssemblyInfo` is off: assembly attributes stay in each project's `Properties/AssemblyInfo.cs`.
- Packages are referenced per project (no `packages.config`, no central package management).
- Layering: `Domain` <- `DataAccess` / `Core` <- `host`. `Transport` holds DTOs shared by server and client. Keep domain code free of transport and persistence types.
- Transport is SignalR (`OrderHub`). Hub methods cannot be overloaded, so each request type has its own method name; shared names are in `OrderHubMethods`.
- `Oms.Server.Domain/Workflow/OrderStateMachine.cs` is generated from `OrderStateMachine.stateless`: edit the designer file, not the C#. Custom logic goes in `OrderStateMachine.Custom.cs`.

## Known issues

- `Oms.Server.Domain.Tests` has 4 failing tests (order state resolves to `Undefined`); they predate or were unverified by the .NET 10 migration.
- `OrderServiceContract.Handle*` still return `null` (work in progress).
