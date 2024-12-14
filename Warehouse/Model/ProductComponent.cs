using System.ComponentModel;
using System.Data;

namespace Warehouse.Model
{
    public class ProductComponent : Component
    {
        [DisplayName("Требуется")]
        public int Required { get; set; }

        new static public ProductComponent FromDataRow(DataRow row)
        {
            var result = new ProductComponent();
            Converter.Fill(result, row);
            return result;
        }

        static public new string GetHeader()
        {
            return "id\tНаименование\tТип\tТребуется\tНаличие\tИспользуется\tОстаток\tЦена\tЗаказано\tДата поставки\tЗаметки";
        }

        public override string ToString()
        {
            var e = ExpectedDate == null ? string.Empty : ExpectedDate.Value.ToString("yyyy - MM - dd");
            return $"{Id}\t{Name}\t{Type}\t{Required}\t{Amount}\t{AmountInUse}\t{Remainder}\t{Price}\t{Ordered}\t{e}\t{Details}";
        }
    }
}
