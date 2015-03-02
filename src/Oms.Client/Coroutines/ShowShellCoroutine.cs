using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Oms.Client.ViewModels;

namespace Oms.Client.Coroutines
{
    class ShowShellCoroutine : IResult
    {
        private readonly Type screenType;
        private string name;

        public ShowShellCoroutine(string name)
        {
            this.name = name;
        }

        public ShowShellCoroutine(Type screenType)
        {
            this.screenType = screenType;
        }

        [Import]
        public IShell Shell { get; set; }

        public void Execute(CoroutineExecutionContext context)
        {
            Completed(this, new ResultCompletionEventArgs());
        }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate {};
    }
}
