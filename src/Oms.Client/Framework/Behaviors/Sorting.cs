using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Oms.Client.Framework.Behaviors
{
    public class Sorting
    {
        private ListSortDirection _sortDirection;
        private GridViewColumnHeader _sortColumn;

        private string SetAdorner(object columnHeader)
        {
            var column = columnHeader as GridViewColumnHeader;
            if (column == null)
            {
                return null;
            }
            if (_sortColumn != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(_sortColumn);
                var adorners = adornerLayer.GetAdorners(_sortColumn);
                if (adorners != null && adorners.Length > 0)
                    adornerLayer.Remove(adorners[0]);
            }

            if (Equals(_sortColumn, column))
            {
                _sortDirection = _sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
            else
            {
                _sortColumn = column;
                _sortDirection = ListSortDirection.Ascending;
            }

            var sortingAdorner = new SortingAdorner(column, _sortDirection);
            AdornerLayer.GetAdornerLayer(column).Add(sortingAdorner);
            var header = string.Empty;
            var b = _sortColumn.Column.DisplayMemberBinding as Binding;
            if (b != null)
            {
                header = b.Path.Path;
            }

            return header;
        }

        public void Sort(object columnHeader, CollectionView list)
        {
            var column = SetAdorner(columnHeader);
            if (column == null)
            {
                return;
            }
            list.SortDescriptions.Clear();
            list.SortDescriptions.Add(new SortDescription(column, _sortDirection));
            if (list.GroupDescriptions == null) return;
            foreach (var groupColumn in list.GroupDescriptions.OfType<PropertyGroupDescription>().Select(g => g.PropertyName).Where(n => !n.Equals(column)))
            {
                list.SortDescriptions.Add(new SortDescription(groupColumn, ListSortDirection.Descending));
            }
        }
    } 
}
