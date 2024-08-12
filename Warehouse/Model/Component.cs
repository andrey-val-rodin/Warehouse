using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Warehouse.Model
{
    public class Component : ICloneable
    {
        [DisplayName("id")]
        [Key]
        public int Id { get; set; }

        [DisplayName("Наименование")]
        [MinLength(1), MaxLength(40)]
        public string Name { get; set; }

        [DisplayName("Тип")]
        [Range(1, 20)]
        public int Type { get; set; }

        [DisplayName("Наличие")]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }

        [DisplayName("Остаток")]
        public int Remainder { get; set; }

        [DisplayName("Цена")]
        public decimal? Price { get; set; }

        [DisplayName("Заказано")]
        [Range(0, int.MaxValue)]
        public int? Ordered { get; set; }

        [DisplayName("Дата поставки")]
        public DateTime? ExpectedDate { get; set; }

        [DisplayName("Заметки")]
        [MinLength(1), MaxLength(1024)]
        public string Details { get; set; }

        static public Component FromDataRow(DataRow row)
        {
            var result = new Component();
            Converter.Fill(result, row);
            return result;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
