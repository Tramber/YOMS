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
- Trades reach the hub through `OrderTradeRequest` (`AddTrade`, `CancelTrade`, `BookTrade`), but there is no booking system: `BookTrade` forces a new trade to booked by hand. The client's Execute button trades what remains of an order at its limit price; there is no form to enter a quantity or an amount.
- The SignalR layer is tested in `Oms.Server.host.Tests`: the real client `OrderServiceProxy` against the host running in memory (`WebApplicationFactory`, long polling, a SQLite file), a check that every `OrderHubMethods` name exists on `OrderHub`, and `LiveSignalRTests`, the same host on a real loopback port (Kestrel) over WebSockets, including Unsubscribe. Keep `Program` public (`public partial class Program`) for it. The Avalonia UI itself, and the pushed-change path onto the UI thread, are not tested, and the client has not been run by hand against a live server.
- Editing an existing order: the blotter's Edit button opens the editor on it and sends `EditionCommand.Update`. The server makes an update pending (status `UpdatePending`, values unchanged) until someone presses Accept pending or Reject pending, so the blotter only shows the new values after the acceptance. The edit form does not cover stop prices, settlement or expiry dates.

## GitHub issues

One problem or task per issue, so each can be closed by one PR. Search the open issues first to avoid duplicates.

**Title:** a short statement of the problem or the missing capability, in the present tense, no prefix and no trailing period ("Client cannot update an existing order", not "[BUG] update"). Under about 70 characters.

**Body**, in this order, leaving out a section that has nothing to say:

1. **Context:** what is wrong or missing, and where. Name classes, methods, settings and layers (`OrderHub`, `Oms.Client`), not file dumps. Say what happens today and what is expected.
2. **Steps to reproduce:** numbered, with the exact command or input (bugs only).
3. **Actual / Expected:** the observed behaviour with the exact error text, and what should happen instead (bugs only).
4. **Impact:** who or what is affected and how badly (data loss, wrong result, missing feature, warning only).
5. **To do:** a checklist (`- [ ]`) of concrete steps, small enough that each is obvious. Mention the tests to add.
6. **Acceptance criteria:** how we know it is done (a behaviour or a passing test), when the To do list alone does not say.
7. **References:** related issues (`#12`), PRs, advisories or docs.

**Labels:** `bug` (wrong behaviour), `enhancement` (new capability), `tech-debt` (internal cleanup, tests, CI, warnings), `documentation`. Use one type label; add `security` when it is a vulnerability or an access problem.

**Lifecycle:**
- A PR that fixes an issue says `Closes #n` in its description.
- Keep the "Known issues" list above in sync: open an issue for each entry and remove the entry when the issue is closed.
- Edit the issue instead of adding a comment when its scope changes; use comments for discussion and progress.
- Do not put secrets, personal data or customer content in an issue.

**Example**

> **Title:** Trades are unreachable from the hub
>
> **Context:** Trades work in the domain (`AddTrade`, `CancelTrade`, the booking steps) but no hub command reaches them, so a client cannot book executions.
>
> **Impact:** Orders can never be filled from a client; feature missing.
>
> **To do:**
> - [ ] Add trade commands to `IOrderServiceContract`, `OrderHub` and `OrderManagementService`
> - [ ] Add tests at service and contract level
> - [ ] Add the matching client actions
>
> **Acceptance criteria:** a client can book an execution and the order reaches `Filled`.
>
> **References:** #12
