using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;

namespace Oms.Client.Controls.Observables
{
    public class BindableCollectionEx<T> : BindableCollection<T>
    {
        public void Refresh(IList<T> items)
        {
            using (new BindableCollectionNotifier<T>(this))
            {
                foreach (var item in items)
                {
                    var index = base.IndexOf(item);
                    if (index < 0)
                        continue;
                    base.SetItem(index, item);
                }
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        private class BindableCollectionNotifier<T> : IDisposable
        {
            private readonly bool _previousNotificationSetting;
            private readonly BindableCollection<T> _collection;

            public BindableCollectionNotifier(BindableCollection<T> collection)
            {
                _previousNotificationSetting = collection.IsNotifying;
                collection.IsNotifying = false;
                this._collection = collection;
            }

            public void Dispose()
            {
                this._collection.IsNotifying = _previousNotificationSetting;
            }
        }
    }
}
