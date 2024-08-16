using System.Globalization;
using System.Windows.Controls;
using Warehouse.Model;

namespace Warehouse.View
{
    /*
    1	ПС.400.Фото
    2	ПС.400.Видео
    3	ПС.400.Пульт
    4	ПС.400.Корпус
    5	ПС.600.Фото
    6	ПС.600.Видео
    7	ПС.600.Пульт
    8	ПС.600.Корпус
    9	ПС.900.Фото
    10	ПС.900.Видео
    11	ПС.900.Пульт
    12	ПС.900.Корпус
    13	ПС.1200.Фото
    14	ПС.1200.Видео
    15	ПС.1200.Пульт
    16	ПС.1200.Корпус
    17	ПС.1500.Фото
    18	ПС.1500.Видео
    19	ПС.1500.Пульт
    20	ПС.1500.Корпус
    */
    public class TableIdValidationRule : ValidationRule
    {
        public static int[] BluetoothTables = [1, 2, 5, 6, 9, 10, 13, 14, 17, 18];
        static public Fabrication Fabrication { get; set; }

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
                    BluetoothTables.Contains(Fabrication.ProductId))
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