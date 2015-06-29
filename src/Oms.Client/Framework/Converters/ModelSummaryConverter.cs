using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Oms.Client.Model;

namespace Oms.Client.Framework.Converters
{
    public class ModelSummaryConverter<T, TM> : IValueConverter where T : IModelAdapterSummary<TM>, new()
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as IList;
            var result = new T();
            if (collection == null)
            {
                return result;
            }
            result.Compute(collection.OfType<TM>().ToList());
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrderAdapterSummaryConverter : ModelSummaryConverter<OrderAdapterSummary, OrderAdapter>
    {
        
    }

    public interface IModelAdapterSummary<T>
    {
        void Compute(ICollection<T> orders);
    }

    public class OrderAdapterSummary : OrderAdapter, IModelAdapterSummary<OrderAdapter>
    {
        public void Compute(ICollection<OrderAdapter> orders)
        {
            base.GroupId = orders.First().GroupId;
            base.Price = orders.Select(o => o.Price).Max();
            base.Quantity = orders.Select(o => o.Quantity).Sum();
            base.Id = orders.Count;
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded == value)
                    return;
                _isExpanded = value;
                base.NotifyOfPropertyChange(() => this.IsExpanded);
            }
        }
    }



}
