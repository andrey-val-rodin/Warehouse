using System.Globalization;
using System.Windows.Data;
using Warehouse.Model;

namespace Warehouse.View
{
    class OverdueDateConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new OverdueDateConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Component c && c.ExpectedDate != null)
                return DateTime.Now > c.ExpectedDate;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
