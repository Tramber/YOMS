using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Oms.Client.Framework.Selectors
{
    public class SimpleDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return DataTemplate;
        }
    }
}
