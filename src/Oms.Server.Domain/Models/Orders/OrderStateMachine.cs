using System;
using System.ComponentModel;
using Oms.Server.Domain.Interfaces.Models;
using Stateless;

namespace Oms.Server.Domain.Models.Orders
{

    public partial class OrderStateMachine
    {
        internal OrderStateMachine(bool fastInitializationAllowed) : this()
        {
            if (fastInitializationAllowed)
            {
                stateMachine.Configure(State.Undefined)
                    .PermitDynamic(new StateMachine<State, Trigger>.TriggerWithParameters<State>(Trigger.Initialize), state => state);
            }
        }

        public void Initialize(State state)
        {
            stateMachine.Fire(Trigger.Initialize);
        }
    }

    public partial class OrderStateMachine
    {
        public delegate void UnhandledTriggerDelegate(State state, Trigger trigger);
        public delegate void EntryExitDelegate();
        public delegate bool GuardClauseDelegate();

        public enum Trigger
        {
            Initialize,
            TakeSnapshot,
            ApplySnapshot,
            Create,
            Cancel,
            Delete,
            Recall,
            StartBooking,
            AddTrade,
            CancelTrade,
            SendRequest,
            SendReject,
            SendAccept,
            MarketSend,
            MarketCancel,
            TradeBooked,
        }

        public enum State
        {
            Undefined,
            Draft,
            Accepting,
            Working,
            Dealing,
            Validating,
            Booking,
            Terminated,
        }

        private readonly StateMachine<State, Trigger> stateMachine = null;

        public EntryExitDelegate OnInitializeEntry = null;
        public EntryExitDelegate OnInitializeExit = null;
        public EntryExitDelegate OnDraftEntry = null;
        public EntryExitDelegate OnDraftExit = null;
        public EntryExitDelegate OnAcceptingEntry = null;
        public EntryExitDelegate OnAcceptingExit = null;
        public EntryExitDelegate OnWorkingEntry = null;
        public EntryExitDelegate OnWorkingExit = null;
        public EntryExitDelegate OnDealingEntry = null;
        public EntryExitDelegate OnDealingExit = null;
        public EntryExitDelegate OnValidatingEntry = null;
        public EntryExitDelegate OnValidatingExit = null;
        public EntryExitDelegate OnBookingEntry = null;
        public EntryExitDelegate OnBookingExit = null;
        public EntryExitDelegate OnTerminatedEntry = null;
        public EntryExitDelegate OnTerminatedExit = null;
        public GuardClauseDelegate GuardClauseFromDraftToAcceptingUsingTriggerSendRequest = null;
        public GuardClauseDelegate GuardClauseFromDraftToTerminatedUsingTriggerDelete = null;
        public GuardClauseDelegate GuardClauseFromAcceptingToDraftUsingTriggerSendReject = null;
        public GuardClauseDelegate GuardClauseFromAcceptingToTerminatedUsingTriggerSendReject = null;
        public GuardClauseDelegate GuardClauseFromAcceptingToWorkingUsingTriggerSendAccept = null;
        public GuardClauseDelegate GuardClauseFromAcceptingToTerminatedUsingTriggerCancel = null;
        public GuardClauseDelegate GuardClauseFromAcceptingToDraftUsingTriggerRecall = null;
        public GuardClauseDelegate GuardClauseFromWorkingToTerminatedUsingTriggerCancel = null;
        public GuardClauseDelegate GuardClauseFromWorkingToDealingUsingTriggerAddTrade = null;
        public GuardClauseDelegate GuardClauseFromWorkingToValidatingUsingTriggerAddTrade = null;
        public GuardClauseDelegate GuardClauseFromWorkingToDealingUsingTriggerMarketSend = null;
        public GuardClauseDelegate GuardClauseFromDealingToTerminatedUsingTriggerCancel = null;
        public GuardClauseDelegate GuardClauseFromDealingToDealingUsingTriggerAddTrade = null;
        public GuardClauseDelegate GuardClauseFromDealingToValidatingUsingTriggerAddTrade = null;
        public GuardClauseDelegate GuardClauseFromDealingToDealingUsingTriggerCancelTrade = null;
        public GuardClauseDelegate GuardClauseFromDealingToDealingUsingTriggerMarketSend = null;
        public GuardClauseDelegate GuardClauseFromDealingToDealingUsingTriggerMarketCancel = null;
        public GuardClauseDelegate GuardClauseFromValidatingToTerminatedUsingTriggerCancel = null;
        public GuardClauseDelegate GuardClauseFromValidatingToBookingUsingTriggerStartBooking = null;
        public GuardClauseDelegate GuardClauseFromValidatingToDealingUsingTriggerCancelTrade = null;
        public GuardClauseDelegate GuardClauseFromValidatingToValidatingUsingTriggerCancelTrade = null;
        public GuardClauseDelegate GuardClauseFromBookingToBookingUsingTriggerCancelTrade = null;
        public GuardClauseDelegate GuardClauseFromBookingToDealingUsingTriggerCancelTrade = null;
        public GuardClauseDelegate GuardClauseFromBookingToBookingUsingTriggerTradeBooked = null;
        public GuardClauseDelegate GuardClauseFromBookingToTerminatedUsingTriggerTradeBooked = null;
        public GuardClauseDelegate GuardClauseFromTerminatedToDealingUsingTriggerAddTrade = null;
        public UnhandledTriggerDelegate OnUnhandledTrigger = null;

        public OrderStateMachine()
        {
            stateMachine = new StateMachine<State, Trigger>(State.Undefined);
            stateMachine.Configure(State.Undefined)
                .OnEntry(() => { if (OnInitializeEntry != null) OnInitializeEntry(); })
                .OnExit(() => { if (OnInitializeExit != null) OnInitializeExit(); })
                ;
            stateMachine.Configure(State.Draft)
                .OnEntry(() => { if (OnDraftEntry != null) OnDraftEntry(); })
                .OnExit(() => { if (OnDraftExit != null) OnDraftExit(); })
                .PermitIf(Trigger.SendRequest, State.Accepting, () => { if (GuardClauseFromDraftToAcceptingUsingTriggerSendRequest != null) return GuardClauseFromDraftToAcceptingUsingTriggerSendRequest(); return true; })
                .PermitIf(Trigger.Delete, State.Terminated, () => { if (GuardClauseFromDraftToTerminatedUsingTriggerDelete != null) return GuardClauseFromDraftToTerminatedUsingTriggerDelete(); return true; })
                ;
            stateMachine.Configure(State.Accepting)
                .OnEntry(() => { if (OnAcceptingEntry != null) OnAcceptingEntry(); })
                .OnExit(() => { if (OnAcceptingExit != null) OnAcceptingExit(); })
                .PermitIf(Trigger.SendReject, State.Draft, () => { if (GuardClauseFromAcceptingToDraftUsingTriggerSendReject != null) return GuardClauseFromAcceptingToDraftUsingTriggerSendReject(); return true; })
                .PermitIf(Trigger.SendReject, State.Terminated, () => { if (GuardClauseFromAcceptingToTerminatedUsingTriggerSendReject != null) return GuardClauseFromAcceptingToTerminatedUsingTriggerSendReject(); return true; })
                .PermitIf(Trigger.SendAccept, State.Working, () => { if (GuardClauseFromAcceptingToWorkingUsingTriggerSendAccept != null) return GuardClauseFromAcceptingToWorkingUsingTriggerSendAccept(); return true; })
                .PermitIf(Trigger.Cancel, State.Terminated, () => { if (GuardClauseFromAcceptingToTerminatedUsingTriggerCancel != null) return GuardClauseFromAcceptingToTerminatedUsingTriggerCancel(); return true; })
                .PermitIf(Trigger.Recall, State.Draft, () => { if (GuardClauseFromAcceptingToDraftUsingTriggerRecall != null) return GuardClauseFromAcceptingToDraftUsingTriggerRecall(); return true; })
                ;
            stateMachine.Configure(State.Working)
                .OnEntry(() => { if (OnWorkingEntry != null) OnWorkingEntry(); })
                .OnExit(() => { if (OnWorkingExit != null) OnWorkingExit(); })
                .PermitIf(Trigger.Cancel, State.Terminated, () => { if (GuardClauseFromWorkingToTerminatedUsingTriggerCancel != null) return GuardClauseFromWorkingToTerminatedUsingTriggerCancel(); return true; })
                .PermitIf(Trigger.AddTrade, State.Dealing, () => { if (GuardClauseFromWorkingToDealingUsingTriggerAddTrade != null) return GuardClauseFromWorkingToDealingUsingTriggerAddTrade(); return true; })
                .PermitIf(Trigger.AddTrade, State.Validating, () => { if (GuardClauseFromWorkingToValidatingUsingTriggerAddTrade != null) return GuardClauseFromWorkingToValidatingUsingTriggerAddTrade(); return true; })
                .PermitIf(Trigger.MarketSend, State.Dealing, () => { if (GuardClauseFromWorkingToDealingUsingTriggerMarketSend != null) return GuardClauseFromWorkingToDealingUsingTriggerMarketSend(); return true; })
                ;
            stateMachine.Configure(State.Dealing)
                .OnEntry(() => { if (OnDealingEntry != null) OnDealingEntry(); })
                .OnExit(() => { if (OnDealingExit != null) OnDealingExit(); })
                .PermitIf(Trigger.Cancel, State.Terminated, () => { if (GuardClauseFromDealingToTerminatedUsingTriggerCancel != null) return GuardClauseFromDealingToTerminatedUsingTriggerCancel(); return true; })
                .PermitReentryIf(Trigger.AddTrade, () => { if (GuardClauseFromDealingToDealingUsingTriggerAddTrade != null) return GuardClauseFromDealingToDealingUsingTriggerAddTrade(); return true; })
                .PermitIf(Trigger.AddTrade, State.Validating, () => { if (GuardClauseFromDealingToValidatingUsingTriggerAddTrade != null) return GuardClauseFromDealingToValidatingUsingTriggerAddTrade(); return true; })
                .PermitReentryIf(Trigger.CancelTrade, () => { if (GuardClauseFromDealingToDealingUsingTriggerCancelTrade != null) return GuardClauseFromDealingToDealingUsingTriggerCancelTrade(); return true; })
                .PermitReentryIf(Trigger.MarketSend, () => { if (GuardClauseFromDealingToDealingUsingTriggerMarketSend != null) return GuardClauseFromDealingToDealingUsingTriggerMarketSend(); return true; })
                .PermitReentryIf(Trigger.MarketCancel, () => { if (GuardClauseFromDealingToDealingUsingTriggerMarketCancel != null) return GuardClauseFromDealingToDealingUsingTriggerMarketCancel(); return true; })
                ;
            stateMachine.Configure(State.Validating)
                .OnEntry(() => { if (OnValidatingEntry != null) OnValidatingEntry(); })
                .OnExit(() => { if (OnValidatingExit != null) OnValidatingExit(); })
                .PermitIf(Trigger.Cancel, State.Terminated, () => { if (GuardClauseFromValidatingToTerminatedUsingTriggerCancel != null) return GuardClauseFromValidatingToTerminatedUsingTriggerCancel(); return true; })
                .PermitIf(Trigger.StartBooking, State.Booking, () => { if (GuardClauseFromValidatingToBookingUsingTriggerStartBooking != null) return GuardClauseFromValidatingToBookingUsingTriggerStartBooking(); return true; })
                .PermitIf(Trigger.CancelTrade, State.Dealing, () => { if (GuardClauseFromValidatingToDealingUsingTriggerCancelTrade != null) return GuardClauseFromValidatingToDealingUsingTriggerCancelTrade(); return true; })
                .PermitReentryIf(Trigger.CancelTrade, () => { if (GuardClauseFromValidatingToValidatingUsingTriggerCancelTrade != null) return GuardClauseFromValidatingToValidatingUsingTriggerCancelTrade(); return true; })
                ;
            stateMachine.Configure(State.Booking)
                .OnEntry(() => { if (OnBookingEntry != null) OnBookingEntry(); })
                .OnExit(() => { if (OnBookingExit != null) OnBookingExit(); })
                .PermitReentryIf(Trigger.CancelTrade, () => { if (GuardClauseFromBookingToBookingUsingTriggerCancelTrade != null) return GuardClauseFromBookingToBookingUsingTriggerCancelTrade(); return true; })
                .PermitIf(Trigger.CancelTrade, State.Dealing, () => { if (GuardClauseFromBookingToDealingUsingTriggerCancelTrade != null) return GuardClauseFromBookingToDealingUsingTriggerCancelTrade(); return true; })
                .PermitReentryIf(Trigger.TradeBooked, () => { if (GuardClauseFromBookingToBookingUsingTriggerTradeBooked != null) return GuardClauseFromBookingToBookingUsingTriggerTradeBooked(); return true; })
                .PermitIf(Trigger.TradeBooked, State.Terminated, () => { if (GuardClauseFromBookingToTerminatedUsingTriggerTradeBooked != null) return GuardClauseFromBookingToTerminatedUsingTriggerTradeBooked(); return true; })
                ;
            stateMachine.Configure(State.Terminated)
                .OnEntry(() => { if (OnTerminatedEntry != null) OnTerminatedEntry(); })
                .OnExit(() => { if (OnTerminatedExit != null) OnTerminatedExit(); })
                .PermitIf(Trigger.AddTrade, State.Dealing, () => { if (GuardClauseFromTerminatedToDealingUsingTriggerAddTrade != null) return GuardClauseFromTerminatedToDealingUsingTriggerAddTrade(); return true; })
                ;
            stateMachine.OnUnhandledTrigger((state, trigger) => { if (OnUnhandledTrigger != null) OnUnhandledTrigger(state, trigger); });
        }

        public bool TryFireTrigger(Trigger trigger)
        {
            if (!stateMachine.CanFire(trigger))
            {
                return false;
            }
            stateMachine.Fire(trigger);
            return true;
        }

        public State GetState
        {
            get
            {
                return stateMachine.State;
            }
        }
    }
}