using System.ComponentModel;
using System.Data;

namespace Warehouse.Model
{
    public class Component
    {
        [DisplayName("id")]
        public int Id { get; set; }

        [DisplayName("Тип")]
        public int Type { get; set; }

        [DisplayName("Наименование")]
        public string Name { get; set; }

        [DisplayName("Наличие")]
        public int Amount { get; set; }

        [DisplayName("Цена")]
        public decimal? Price { get; set; }

        static public Component FromDataRow(DataRow row)
        {
            var result = new Component();
            Converter.Fill(result, row);
            return result;
        }
    }
}
