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
    }
}
