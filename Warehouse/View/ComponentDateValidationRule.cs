using System.Globalization;
using System.Windows.Controls;
using Warehouse.Model;

namespace Warehouse.View
{
    public class ComponentDateValidationRule : ValidationRule
    {
        public static Component Component { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is DateTime date)
            {
                // Check date range
                var min = DateTime.Now.Date;
                var max = (min + TimeSpan.FromDays(180)).Date;

                if (date < min || date > max)
                    return new ValidationResult(false, "Дата вне допустимого диапазона.");
            }
            else if (Component.Ordered != null)
            {
                return new ValidationResult(false, "Нужно ввести дату получения заказа.");
            }

            return new ValidationResult(true, null);
        }
    }
}