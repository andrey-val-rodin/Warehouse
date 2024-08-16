using System.Globalization;
using System.Windows.Controls;

namespace Warehouse.View
{
    public class PriceValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string input)
                return new ValidationResult(false, "Внутренняя ошибка преобразования.");

            if (string.IsNullOrEmpty(input))
                return new ValidationResult(true, null);

            // Is a number?
            var style = NumberStyles.Number;
            var culture = CultureInfo.InvariantCulture;
            if (!decimal.TryParse(input, style, culture, out decimal number))
                return new ValidationResult(false, "Это не число.");

            // Is positive?
            if (number < 0)
                return new ValidationResult(false, "Цена не может быть отрицательной.");

            // Has decimal point?
            if (input.Contains('.'))
            {
                // Must be two digits after dot
                var pos = input.IndexOf('.');
                if (input.Length - pos != 3)
                    return new ValidationResult(false, "Неверно указаны копейки.");
            }

            // Number is valid
            return new ValidationResult(true, null);
        }
    }
}