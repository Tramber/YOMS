using Stateless;

namespace Oms.Server.Domain.Workflow
{
    public partial class OrderStateMachine
    {
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<State> _initializeTrigger = new StateMachine<State, Trigger>.TriggerWithParameters<State>(Trigger.Initialize);

        /// <summary>Creates a state machine in Undefined that can be moved to any state with <see cref="Initialize"/>, to resume an order from its event log.</summary>
        public static OrderStateMachine CreateInitializable()
        {
            var result = new OrderStateMachine();
            result._stateMachine.Configure(State.Undefined).PermitDynamic(result._initializeTrigger, state => state);
            return result;
        }

        /// <summary>Puts the machine in the given state without running entry or exit actions. Only valid while it is still Undefined.</summary>
        public void Initialize(State state)
        {
            _stateMachine.Fire(_initializeTrigger, state);
        }

        /// <summary>True when the trigger is allowed in the current state.</summary>
        public bool CanFireTrigger(Trigger trigger)
        {
            return _stateMachine.CanFire(trigger);
        }
    }
}
