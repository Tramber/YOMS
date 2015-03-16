using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.ComponentModel;
using System.Reactive.Concurrency;
using Oms.Client.Framework.Observables;

namespace Oms.Client.ViewModels
{
    [Export(typeof (IOrderEditor))]
    public class OrderEditorViewModel : Screen, IOrderEditor
    {
        private string _searchText;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            LogData = new BindableCollection<string>();
            SearchResults = new BindableCollectionEx<string>();

            var searchTextChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                ev => PropertyChanged += ev,
                ev => PropertyChanged -= ev)
                .Where(ev => ev.EventArgs.PropertyName == "SearchText")
                .Select(_ => SearchText);
            var input = searchTextChanged
                .Where(s => s == null || s.Length < 3)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Merge(searchTextChanged
                    .Where(s => s != null && s.Length >= 3)
                    .Throttle(TimeSpan.FromMilliseconds(300)))
                .DistinctUntilChanged();

            //for logging purpose
            var logSubscriber = input
                .ObserveOn(Scheduler.Default)
                .SubscribeOn(Scheduler.CurrentThread)
                .Subscribe(a => LogData.Add(a.ToString()));

            //The search in the repository
            var search = Observable.ToAsync<string, SearchResult>(DoSearch);
            var results = input.SelectMany(searchTerm => search(searchTerm).TakeUntil(input));
            var resultSubscriber = results
                .ObserveOn(Scheduler.Default)
                .SubscribeOn(Scheduler.CurrentThread)
                .Subscribe(r => SearchResults.Reload(r.Results.ToList()));

        }


        public void Close()
        {
            TryClose();
        }

        public void Search()
        {

        }

        public BindableCollection<string> LogData { get; private set; }

        public BindableCollectionEx<string> SearchResults { get; private set; } 

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText) return;
                _searchText = value;
                NotifyOfPropertyChange();
            }
        }

        private SearchResult DoSearch(string searchTerm)
        {
            return new SearchResult(searchTerm,
                string.IsNullOrWhiteSpace(searchTerm)
                    ? new String[0]
                    : phrases.Where(item => item.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant())).OrderBy(s => s).ToArray());
        }

        private readonly string[] phrases = new[]
        {
           "Forums",
           "Blog",
           "Facebook",
           "LinkedIn",
           "Stack Overflow",
           "Twitter",
           "Visual Studio Events",
           "YouTube"
        };
    }


    public struct SearchResult : IEquatable<SearchResult>
    {

        public SearchResult(string searchTerm, IEnumerable<string> results) : this()
        {
            SearchTerm = searchTerm;
            Results = results;
        }

        public string SearchTerm { get; set; }
        public IEnumerable<string> Results { get; set; }

        public bool Equals(SearchResult other)
        {
            return string.Equals(SearchTerm, other.SearchTerm);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SearchResult && Equals((SearchResult) obj);
        }

        public override int GetHashCode()
        {
            return (SearchTerm != null ? SearchTerm.GetHashCode() : 0);
        }
    }
}
