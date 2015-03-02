using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Oms.Client.Controls.Behaviors
{
    public class SortingBehavior : Behavior<ListView>
    {
        private readonly Sorting _sorting;

        public SortingBehavior()
        {
            _sorting = new Sorting();
        }

        protected override void OnAttached()
        {
            AssociatedObject.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClicked));
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClicked));
        }

        private void OnColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null)
            {
                return;
            }
            _sorting.Sort(e.OriginalSource, listView.Items);
        }

    } 
}
