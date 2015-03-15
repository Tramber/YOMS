using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Oms.Client.Framework.Converters
{
    public class CollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible;
            ICollection collection = value as ICollection;
            if (collection != null)
            {
                isVisible = collection.Count > 0;
            }
            else if (value is int)
            {
                isVisible = ((int) value) > 0;
            }
            else if (value is long)
            {
                isVisible = ((long)value) > 0;
            }
            else
            {
                isVisible = false;
            }
            if (targetType == typeof (bool))
            {
                return isVisible;
            }
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }




        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
