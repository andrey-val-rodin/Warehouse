using ExcelDataReader;

namespace WarehouseFiller
{
    public static class ExcelDataReaderExtensions
    {
        public static bool IsGroup(this IExcelDataReader reader)
        {
            for (int col = 1; col < reader.FieldCount; col++)
            {
                if (reader.GetValue(col) != null)
                    return false;
            }

            return true;
        }

        public static int? GetIntValue(this IExcelDataReader reader, int column)
        {
            if (reader.GetValue(column) == null)
                return null;

            return (int)reader.GetDouble(column);
        }
    }
}
