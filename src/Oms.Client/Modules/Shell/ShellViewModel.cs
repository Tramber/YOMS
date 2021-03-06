﻿using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Oms.Client.Framework.Observables;
using Oms.Client.Modules.Notification;
using Oms.Client.Modules.OrderBlotter;
using Oms.Client.Modules.OrderEditor;
using Oms.Client.Services.Notifications;

namespace Oms.Client.Modules.Shell
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<INotification>
    {
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly INotificationService _notificationService;

        [ImportingConstructor]
        public ShellViewModel(IWindowManager windowManager,
            IEventAggregator eventAggregator,
            INotificationService notificationService)
        {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _notificationService = notificationService;
            Notifications = new BindableCollectionEx<NotificationViewModel>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Items.Add(new OrderBlotterViewModel());
            Items.First().Activate();

            _eventAggregator.Subscribe(this);
        }

        public IObservableCollection<IScreen> Documents
        {
            get { return Items; }
        }

        public IObservableCollection<IScreen> Tools
        {
            get { return null; }
        }

        public void CreateOrder()
        {
            _windowManager.ShowDialog(new OrderEditorViewModel());
        }

        public void CreateNotification()
        {
            _notificationService.ProcessInfo("hello", "Ding Ding Dong");
        }

        public void Handle(INotification message)
        {
            Notifications.Add(new NotificationViewModel(message));
        }

        public IObservableCollection<NotificationViewModel> Notifications { get; private set; } 
    }
}
