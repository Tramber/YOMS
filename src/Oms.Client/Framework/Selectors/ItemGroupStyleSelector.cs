using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Oms.Client.Model;

namespace Oms.Client.Framework.Selectors
{
    public class ItemGroupStyleSelector : StyleSelector
    {
        public Style GroupStyle { get; set; }
        public Style OrphanStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var list = item as CollectionViewGroup;
            
            return 
                list == null || list.Items.OfType<OrderAdapter>().Any(o => o.GroupId != 0) ? GroupStyle : OrphanStyle;
        }
    }
       
}

