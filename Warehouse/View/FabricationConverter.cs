using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Warehouse.Database;
using Warehouse.Model;

namespace Warehouse.View
{
    class FabricationConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new FabricationConverter();
        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Fabrication f)
            {
                // level 1 - overdue order
                if (f.ExpectedDate != null && DateTime.Now > f.ExpectedDate)
                    return 1;

                // level 2 - insufficient amount
                if (SqlProvider.HasNegativeRemainders(f.ProductId))
                    return 2;

                // level 3 - no price
                // not applicable
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
