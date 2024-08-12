using System.Globalization;
using System.Windows.Controls;

namespace Warehouse.View
{
    public class OrderedValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string input)
                return new ValidationResult(false, "Внутренняя ошибка преобразования.");

            if (string.IsNullOrEmpty(input))
                return new ValidationResult(true, null);

            // Is a number?
            if (!int.TryParse(input, out int number))
                return new ValidationResult(false, "Это не число.");

            // Is positive?
            if (number <= 0)
                return new ValidationResult(false, "Количество должно быть больше 0.");

            // Number is valid
            return new ValidationResult(true, null);
        }
    }
}