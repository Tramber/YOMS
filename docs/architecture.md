# YOMS architecture

YOMS is a small Order Management System: clients submit orders, the server runs each order through a workflow, persists the full history, and pushes changes back to subscribed clients.

## Components

```mermaid
flowchart LR
    Client["Oms.Client<br/>Avalonia"] -- "SignalR /orders" --> Host["Oms.Server.host<br/>OrderHub"]
    Host --> Contract["Oms.Server.Core<br/>OrderServiceContract"]
    Contract --> Mgmt["OrderManagementService"]
    Mgmt --> Domain["Oms.Server.Domain<br/>Order, state machine, validation"]
    Mgmt --> Repo["Oms.Server.DataAccess<br/>NHibernate repositories"]
    Repo --> DB[("SQLite")]
    Transport["Oms.Transport<br/>DTOs and messages"] -.shared by.- Client
    Transport -.shared by.- Host
```

| Project | Responsibility |
|---|---|
| `Oms.Transport` | DTOs, request/response messages and `IOrderServiceContract`. The only assembly shared by client and server. |
| `Oms.Server.Domain` | Business model: `Order`, `Trade`, `Security`, `Fund`, `User`; the order workflow; FluentValidation validators; repository interfaces. No persistence or transport types. |
| `Oms.Server.DataAccess` | NHibernate/FluentNHibernate automapping and repositories behind `IRepositoryFacade`. Runs on SQLite through `MicrosoftDataSqliteDriver`. |
| `Oms.Server.Core` | Application layer: `OrderManagementService` maps request commands to domain operations, `DtoMapperExtensions` maps domain to DTOs. |
| `Oms.Server.host` | ASP.NET Core process, DI registration and the SignalR `OrderHub`. |
| `Oms.Client` | Cross-platform Avalonia UI (CommunityToolkit.Mvvm, Microsoft DI). |
| `Oms.Framework` | Task dispatcher queues/bags and other shared utilities. |

## Request flow

1. The client sends a request (`OrderEditionRequest`, `OrderRequest` or `OrderMarketRequest`) to a hub method: `HandleOrderEdition`, `HandleOrder` or `HandleOrderMarket`. SignalR does not allow overloaded hub methods, hence three names.
2. `OrderHub` forwards to `IOrderServiceContract` (`OrderServiceContract`), which calls `OrderManagementService.HandleRequest`.
3. `OrderManagementService` resolves the user from the request token, then looks up the command (`EditionCommand`, `OrderStateCommand`, `OrderMarketCommand`) in `OrderCommandMappings` and invokes the matching `Order` method (`Create`, `SendRequest`, `Update`, `Cancel`, `MarketSend`, ...).
4. Each call fires a trigger on the order's state machine. The result is a `GenericResult`.
5. Clients call `SubscribeToEvents` to join a SignalR group. `OrderServiceContract.OrderListUpdated` is forwarded by the host to that group as `OrderListUpdated(ItemOperationType, OrderDto[])`.

> Work in progress: the three `OrderServiceContract.Handle*` methods call the management service but still return `null`, and nothing raises `OrderListUpdated` yet.

## Order model: event sourcing

An `Order` has no stored state field. It holds a list of `OrderEventLog` entries (`EventLogs`); each records the trigger, its `TriggerStatus`, the resulting `OrderStateMachine.State`, the `TriggerContext` (user and date) and, for data changes, the editable or dealing parameters.

`Order.ComputeOrderData()` replays the non-failed events to rebuild the current `OrderData` (editable data, routing/dealing data, state, pending trigger) and then derives `OrderStatus` (New, PartiallyFilled, Filled, Cancelled, ...). The state machine is re-initialised from that state before each command, so the log is the single source of truth.

## Order workflow

The state machine is designed in `Oms.Server.Domain/Workflow/OrderStateMachine.stateless` (Stateless Designer) and generated into `OrderStateMachine.cs`. Guards are exposed as `GuardClauseFrom<A>To<B>UsingTrigger<T>` delegates, `OrderStateMachine.Custom.cs` adds the initialisation trigger.

```mermaid
stateDiagram-v2
    [*] --> Undefined
    Undefined --> Draft: Create
    Undefined --> Accepting: SendRequest
    Draft --> Accepting: SendRequest
    Draft --> Terminated: Delete
    Draft --> Draft: Update
    Accepting --> Working: SendAccept / Update
    Accepting --> Terminated: SendReject / Cancel
    Accepting --> Draft: Recall
    Working --> Dealing: AddTrade / MarketSend
    Working --> Validating: AddTrade (filled)
    Working --> Terminated: Cancel
    Dealing --> Dealing: AddTrade / CancelTrade / MarketSend / MarketCancel
    Dealing --> Validating: AddTrade (filled)
    Dealing --> Terminated: Cancel
    Validating --> Booking: StartBooking
    Validating --> Dealing: CancelTrade
    Validating --> Validating: CancelTrade
    Validating --> Terminated: Cancel
    Booking --> Booking: CancelTrade / TradeBooked
    Booking --> Dealing: CancelTrade
    Booking --> Terminated: TradeBooked
    Terminated --> Dealing: AddTrade
```

Where two transitions share a trigger, guards in `Order.CreateStateMachine` pick the target:

- `AddTrade` (from Working or Dealing): to Validating when the order is fully executed once the new trade is counted, otherwise to Dealing.
- `CancelTrade` (from Validating or Booking): back to Dealing when cancelling leaves the order not fully executed, otherwise it stays put.
- `TradeBooked` (from Booking): to Terminated when every active trade is booked, otherwise it stays in Booking. An order terminated this way has the status Filled (or OverFilled), not Cancelled.

Cancel and Update are pending commands: they are logged as Pending and only take effect, with the data sent with them, when `AcceptPending` is called. `RejectPending` discards them.

## Trade life cycle

A `Trade` is created by `Order.AddTrade` and records its own event log.

```mermaid
stateDiagram-v2
    [*] --> New: Create
    New --> BookingSent: BookingSend
    BookingError --> BookingSent: BookingSend
    BookingSent --> BookingSent: BookingRefresh
    BookingSent --> Booked: Booked
    BookingSent --> BookingError: BookingFailed
    New --> Booked: ForceBooked
    BookingError --> Booked: ForceBooked
    New --> Cancelled: Cancel
    BookingSent --> Cancelled: Cancel
    BookingError --> Cancelled: Cancel
    Booked --> Cancelled: Cancel
```

## Persistence

`NHibernateHelper` (internal singleton) builds the session factory with FluentNHibernate automapping over the `Order` assembly (`DefaultLazy.Never`, event-log base types ignored) and creates the schema on start-up. The provider comes from the `OmsDB` connection string in `App.config`; SQLite is wired through `Microsoft.Data.Sqlite`, an Oracle branch exists but needs a .NET provider package.

## Known gaps

See `AGENTS.md`. In short: unfinished service responses and notifications, a client that has only been smoke-tested and untested SignalR round trip.
