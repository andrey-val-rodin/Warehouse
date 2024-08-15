using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Data;

namespace Warehouse.Model
{
    public class Fabrication
    {
        [DisplayName("id")]
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [DisplayName("Изделие")]
        public string ProductName { get; set; }

        [DisplayName("Клиент")]
        [StringLength(10000)]
        public string Client { get; set; }

        [DisplayName("Заметки")]
        [StringLength(10000)]
        public string Details { get; set; }

        [DisplayName("id стола")]
        [StringLength(12)]
        public string TableId { get; set; }

        public FabricationStatus Status { get; set; }
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case FabricationStatus.Opened:
                        return "Открыто";
                    case FabricationStatus.Closed:
                        return "Закрыто";
                    case FabricationStatus.Cancelled:
                        return "Отменено";
                    default:
                        return "Unknown";
                }
            }
        }

        [DisplayName("Открыто")]
        public DateTime StartedDate { get; set; }

        [DisplayName("Ожидается")]
        public DateTime? ExpectedDate { get; set; }

        [DisplayName("Закрыто")]
        public DateTime? ClosedDate { get; set; }

        static public Fabrication FromDataRow(DataRow row)
        {
            var result = new Fabrication();
            Converter.Fill(result, row);
            return result;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
