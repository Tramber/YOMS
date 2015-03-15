using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oms.Client.Framework.Caliburn
{
    public static class ObservableEvents
    {
        public static IObservable<PropertyChangedEventArgs> FromPropertyChanged(this INotifyPropertyChanged source)
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(handler => source.PropertyChanged += handler, handler => source.PropertyChanged -= handler).Select(handler => handler.EventArgs);
        }
    }
}
