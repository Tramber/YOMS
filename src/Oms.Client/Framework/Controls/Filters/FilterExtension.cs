using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using log4net.Core;
using log4net.Filter;

namespace Oms.Client.Framework.Controls.Filters
{
    [ContentProperty("Filters")]
    internal class FilterExtension : MarkupExtension
    {
        private readonly ObservableCollection<IPropertyFilter> _filters = new ObservableCollection<IPropertyFilter>();

        public ICollection<IPropertyFilter> Filters
        {
            get { return _filters; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new FilterEventHandler((s, e) =>
            {
                e.Accepted = Filters.Count == 0 || Filters.All(f => f.Filter(e.Item));
            });
        }
    }

    public enum ComparerFilter
    {
        Equal,
        NotEqual,
        In,
        NotIn
    }

    public interface IPropertyFilter
    {
        string PropertyName { get; set; }
        object Value { get; set; }
        bool IsRegex { get; set; }
        ComparerFilter Comparer { get; set; }
        bool Filter(object item);

    }

    public class PropertyFilter : DependencyObject, IPropertyFilter
    {
        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register("PropertyName", typeof(string), typeof(PropertyFilter), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ValueProperty =  DependencyProperty.Register("Value", typeof(object), typeof(PropertyFilter), new UIPropertyMetadata(null));
        public static readonly DependencyProperty IsRegexProperty = DependencyProperty.Register("IsRegex", typeof(bool), typeof(PropertyFilter), new UIPropertyMetadata(false));
        public static readonly DependencyProperty ComparerProperty = DependencyProperty.Register("Comparer", typeof(ComparerFilter), typeof(PropertyFilter), new UIPropertyMetadata(ComparerFilter.Equal));

        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }
       
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        
        public bool IsRegex
        {
            get { return (bool)GetValue(IsRegexProperty); }
            set { SetValue(IsRegexProperty, value); }
        }

        public ComparerFilter Comparer
        {
            get { return (ComparerFilter)GetValue(ComparerProperty); }
            set { SetValue(ComparerProperty, value); }
        }

        public bool Filter(object item)
        {
            var type = item.GetType();
            var itemValue = type.GetProperty(PropertyName).GetValue(item, null);
            return (object.Equals(itemValue, Value));
        }
    }
}
