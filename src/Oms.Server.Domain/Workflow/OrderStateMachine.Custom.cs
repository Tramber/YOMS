using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;

namespace Oms.Server.Domain.Workflow
{
    public partial class OrderStateMachine
    {
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<State> _initializeTrigger = new StateMachine<State, Trigger>.TriggerWithParameters<State>(Trigger.Initialize);

        public static OrderStateMachine CreateInitializable()
        {
            var result = new OrderStateMachine();
            result.stateMachine.Configure(State.Undefined).PermitDynamic(result._initializeTrigger, state => state);
            return result;
        }

        public void Initialize(State state)
        {
            stateMachine.Fire(_initializeTrigger, state);
        }

        public bool CanFireTrigger(Trigger trigger)
        {
            return stateMachine.CanFire(trigger);
        }
    }
}
