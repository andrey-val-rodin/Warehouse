﻿using System.Globalization;
using System.Windows.Data;
using Warehouse.Model;

namespace Warehouse.View
{
    class InsufficientAmountConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new InsufficientAmountConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProductComponent pc)
                return pc.Amount < pc.Required;
            else if (value is Component c)
                return c.Amount <= 1;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
