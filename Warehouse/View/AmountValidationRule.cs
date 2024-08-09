using System.Globalization;
using System.Windows.Controls;

namespace Warehouse.View
{
    public class AmountValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // Is a number?
            if (!int.TryParse((string)value, out int number))
                return new ValidationResult(false, "Это не число.");

            // Is positive?
            if (number < 0)
                return new ValidationResult(false, "Количество не может быть отрицательным.");

            // Number is valid
            return new ValidationResult(true, null);
        }
    }
}