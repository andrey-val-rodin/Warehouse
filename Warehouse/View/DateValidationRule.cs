using System.Globalization;
using System.Windows.Controls;
using Warehouse.Model;

namespace Warehouse.View
{
    public class DateValidationRule : ValidationRule
    {
        public Component Component { get; set; }

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

            return new ValidationResult(true, null);
        }
    }
}