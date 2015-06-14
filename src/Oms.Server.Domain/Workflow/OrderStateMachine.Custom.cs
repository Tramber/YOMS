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
        private StateMachine<State, Trigger>.TriggerWithParameters<State> initializeTrigger;

        internal OrderStateMachine(bool fastInitializationAllowed)
            : this()
        {
            initializeTrigger = new StateMachine<State, Trigger>.TriggerWithParameters<State>(Trigger.Initialize);
            if (fastInitializationAllowed)
            {
                stateMachine.Configure(State.Undefined)
                    .PermitDynamic(initializeTrigger, state => state);
            }
        }

        public void Initialize(State state)
        {
            stateMachine.Fire(initializeTrigger, state);
        }

        public bool CanFireTrigger(Trigger trigger)
        {
            return stateMachine.CanFire(trigger);
        }
    }
}
