using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Oms.Client.Framework.Caliburn
{
    public class ObservableProperty<TValue> : IObservable<TValue>, IObserver<TValue>
    {
        private TValue _current;
        private BehaviorSubject<TValue> _values;
        private IDisposable _notifySubscription;

        public ObservableProperty(TValue defaultValue, INotifyPropertyChangedEx notifier, string propertyName)
        {
            _current = defaultValue;
            _values = new BehaviorSubject<TValue>(defaultValue);
            _notifySubscription = _values.DistinctUntilChanged().Do(value => _current = value).Subscribe(value => notifier.NotifyOfPropertyChange(propertyName));
        }
        public ObservableProperty(INotifyPropertyChangedEx notifier, string propertyName) : this(default(TValue), notifier, propertyName) { }
        public ObservableProperty(TValue defaultValue, INotifyPropertyChangedEx notifier, Expression<Func<TValue>> property) : this(defaultValue, notifier, property.GetMemberInfo().Name) { }
        public ObservableProperty(INotifyPropertyChangedEx notifier, Expression<Func<TValue>> property) : this(default(TValue), notifier, property) { }
        IDisposable IObservable<TValue>.Subscribe(IObserver<TValue> observer)
        {
            return _values.Subscribe(observer);
        }

        void IObserver<TValue>.OnCompleted()
        {
        }

        void IObserver<TValue>.OnError(Exception error)
        {
        }

        void IObserver<TValue>.OnNext(TValue value)
        {
            _values.OnNext(value);
        }

        public TValue Get()
        {
            return _current;
        }

        public void Set(TValue value)
        {
            _values.OnNext(value);
        }
    }
}
