using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Oms.Client.ViewModels;

namespace Oms.Client.Framework.Coroutines
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
