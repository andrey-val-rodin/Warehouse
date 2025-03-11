using System.Globalization;
using System.Windows.Controls;
using Warehouse.Model;

namespace Warehouse.View
{
    public class TableIdValidationRule : ValidationRule
    {
        static public Fabrication Fabrication { get; set; }
        static public Product[] Products;

        static public bool IsBluetoothTable(int productId)
        {
            var product = Products.FirstOrDefault(p => p.Id == Fabrication.ProductId);
            return product != null && product.IsBluetoothTable;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            const string message = @"Введите корректный идентификатор стола.
Идентификатор можно найти в мобильном приложении
на странице Соединение.";

            // Check table id
            if (value is string s && !string.IsNullOrEmpty(s))
            {
                if (!IsValidTableId(s))
                    return new ValidationResult(false, message);
            }
            else
            {
                if (Fabrication.Status == FabricationStatus.Closed &&
                    IsBluetoothTable(Fabrication.ProductId))
                    return new ValidationResult(false, message);
            }

            return new ValidationResult(true, null);
        }

        private static bool IsValidTableId(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            return s.Length == 12 && s.All("0123456789abcdefABCDEF".Contains);
        }
    }
}