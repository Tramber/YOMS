﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Oms.Client.Framework.Caliburn
{
    public class ObservableCommand : ICommand, IDisposable, IObservable<object>
    {
        private Subject<object> _invocations;
        private bool _canExecute;
        private IDisposable _canExecuteSubscription;
        public event EventHandler CanExecuteChanged;
        public ObservableCommand(IObservable<bool> canExecute)
        {
            _invocations = new Subject<object>();
            _canExecuteSubscription = canExecute.DistinctUntilChanged().Subscribe(OnCanExecuteChanged);
        }
        public ObservableCommand() : this(Observable.Return<bool>(true)) { }
        IDisposable IObservable<object>.Subscribe(IObserver<object> observer)
        {
            return _invocations.Subscribe(observer);
        }
        public void Dispose()
        {
            if (_canExecuteSubscription != null)
            {
                _canExecuteSubscription.Dispose();
                _canExecuteSubscription = null;
            }
            if (_invocations != null)
            {
                _invocations.Dispose();
                _invocations = null;
            }
        }
        private void OnCanExecuteChanged(bool canExecute)
        {
            _canExecute = canExecute;
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }
        public void Execute(object parameter)
        {
            _invocations.OnNext(parameter);
        }
    }
}
