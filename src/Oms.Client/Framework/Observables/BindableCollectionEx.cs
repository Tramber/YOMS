using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Oms.Client.Framework.Observables
{
    public class BindableCollectionEx<T> : ObservableCollection<T>
    {
        /// <summary>Raises a single reset so bound views re-evaluate grouping, sorting and filtering.</summary>
        public void Refresh()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reload(IEnumerable<T> items)
        {
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            Refresh();
        }
    }
}
