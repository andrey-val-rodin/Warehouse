using System.Globalization;
using System.Windows.Data;
using Warehouse.Model;

namespace Warehouse.View
{
    class UnitConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new UnitConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Component c)
                return c.IsUnit ? 1 : 0;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
