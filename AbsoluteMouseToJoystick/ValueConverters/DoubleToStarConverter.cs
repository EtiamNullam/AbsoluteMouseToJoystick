using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AbsoluteMouseToJoystick.ValueConverters
{
    /// <summary>
    /// Converts double value to proportional GridLength (Grid star system) to be used in RowDefinition or ColumnDefinition.
    /// </summary>
    public class DoubleToStarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValue = System.Convert.ToDouble(value);
            return new GridLength(convertedValue <= 0 ? 0.0001 : convertedValue, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((GridLength)value).Value;
        }
    }
}
