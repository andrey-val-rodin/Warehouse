using System.Globalization;
using System.Windows.Data;
using Warehouse.Model;

namespace Warehouse.View
{
    class ProblemConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new ProblemConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool priceIsNull = false;
            bool overdueDate = false;
            bool insufficientAmount = false;
            if (value is Component c )
            {
                priceIsNull = c.Price == null;
                overdueDate = c.ExpectedDate != null && DateTime.Now > c.ExpectedDate;
                insufficientAmount = (value is ProductComponent pc) ? pc.Remainder < pc.Required : c.Remainder < 0;
            }

            return priceIsNull || overdueDate || insufficientAmount;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
