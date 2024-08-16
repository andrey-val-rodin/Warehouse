using System.Globalization;
using System.Windows.Data;
using Warehouse.Model;

namespace Warehouse.View
{
    class FabricationConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new FabricationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Fabrication f)
            {
                // level 1 - overdue order
                if (f.ExpectedDate != null && DateTime.Now > f.ExpectedDate)
                    return 1;

                // level 2 - insufficient amount
                // not implemented

                // level 3 - no price
                // not applicable
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
