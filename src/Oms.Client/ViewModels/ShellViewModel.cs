using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Oms.Client.Framework.Themes;
using Oms.Client.Services;

namespace Oms.Client.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell
    {
        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public ShellViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Items.Add(new OrderBlotterViewModel());
            Items.First().Activate();
        }

        public IObservableCollection<IScreen> Documents
        {
            get { return Items; }
        }

        public IObservableCollection<IScreen> Tools
        {
            get { return null; }
        } 
    }
}
