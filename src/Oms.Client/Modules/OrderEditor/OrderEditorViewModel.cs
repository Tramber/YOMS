using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Oms.Client.Services;

namespace Oms.Client.Modules.OrderEditor
{
    public partial class OrderEditorViewModel : ObservableObject, IRequestClose
    {
        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private IReadOnlyList<string> _searchResults = new string[0];

        private readonly string[] _phrases =
        {
            "Forums", "Blog", "Facebook", "LinkedIn", "Stack Overflow", "Twitter", "Visual Studio Events", "YouTube",
        };

        public OrderEditorViewModel()
        {
            var ui = SynchronizationContext.Current;
            var searchTextChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    ev => PropertyChanged += ev,
                    ev => PropertyChanged -= ev)
                .Where(ev => ev.EventArgs.PropertyName == nameof(SearchText))
                .Select(_ => SearchText);

            // Short input waits longer than a real search term before it is used.
            var input = searchTextChanged
                .Where(s => s == null || s.Length < 3)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Merge(searchTextChanged
                    .Where(s => s != null && s.Length >= 3)
                    .Throttle(TimeSpan.FromMilliseconds(300)))
                .DistinctUntilChanged();

            input.ObserveOn(ui)
                .SubscribeOn(Scheduler.CurrentThread)
                .Subscribe(term => LogData.Add(term ?? ""));

            // Search off the UI thread; a new term cancels the pending result of the previous one.
            var search = Observable.ToAsync<string, IReadOnlyList<string>>(DoSearch);
            input.SelectMany(term => search(term).TakeUntil(input))
                .ObserveOn(ui)
                .SubscribeOn(Scheduler.CurrentThread)
                .Subscribe(results => SearchResults = results);
        }

        public ObservableCollection<string> LogData { get; } = new ObservableCollection<string>();

        public event EventHandler CloseRequested;

        [RelayCommand]
        private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

        [RelayCommand]
        private void Search()
        {
        }

        private IReadOnlyList<string> DoSearch(string term)
        {
            return string.IsNullOrWhiteSpace(term)
                ? new string[0]
                : _phrases.Where(p => p.Contains(term, StringComparison.OrdinalIgnoreCase)).OrderBy(p => p).ToArray();
        }
    }
}
