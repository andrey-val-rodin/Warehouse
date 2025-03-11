using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Warehouse.Model
{
    public class Product : ICloneable
    {
        [DisplayName("id")]
        [Key]
        public int Id { get; set; }

        [DisplayName("Наименование")]
        [StringLength(40)]
        public string Name { get; set; }

        public bool IsUnit { get; set; }
        public bool IsBluetoothTable { get; set; }

        static public Product FromDataRow(DataRow row)
        {
            var result = new Product();
            Converter.Fill(result, row);
            return result;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
