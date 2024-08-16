using System.Globalization;
using System.Windows.Data;
using Warehouse.Model;

namespace Warehouse.View
{
    class ComponentConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new ComponentConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProductComponent pc)
            {
                // level 1 - overdue order
                if (pc.ExpectedDate != null && DateTime.Now > pc.ExpectedDate)
                    return 1;

                // level 2 - insufficient amount
                if (pc.Remainder < pc.Required)
                    return 2;

                // level 3 - no price
                if (pc.Price == null)
                    return 3;
            }
            else if (value is Component c)
            {
                // level 1 - overdue order
                if (c.ExpectedDate != null && DateTime.Now > c.ExpectedDate)
                    return 1;

                // level 2 - insufficient amount
                if (c.Remainder < 1)
                    return 2;

                // level 3 - no price
                if (c.Price == null)
                    return 3;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
