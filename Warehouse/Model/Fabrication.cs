using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.RegularExpressions;

namespace Warehouse.Model
{
    public class Fabrication
    {
        [DisplayName("id")]
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [DisplayName("id стола")]
        [StringLength(12)]
        public string TableId { get; set; }

        [DisplayName("Номер")]
        public int Number { get; set; }

        [DisplayName("Статус")]
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

        [DisplayName("Изделие")]
        public string ProductName { get; set; }

        [DisplayName("Клиент")]
        [StringLength(10000)]
        public string Client { get; set; }

        public string ClientPlainText => string.IsNullOrEmpty(Client) ? null : Regex.Replace(Client, @"\t|\n|\r", "");

        [DisplayName("Заметки")]
        [StringLength(10000)]
        public string Details { get; set; }

        public string DetailsPlainText => string.IsNullOrEmpty(Details) ? null : Regex.Replace(Details, @"\t|\n|\r", "");

        [DisplayName("Открыто")]
        public DateTime StartedDate { get; set; }

        [DisplayName("Ожидается")]
        public DateTime? ExpectedDate { get; set; }

        [DisplayName("Закрыто")]
        public DateTime? ClosedDate { get; set; }

        public bool IsUnit { get; set; }

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

        static public string GetHeader()
        {
            return "id\tИзделие\tСтатус\tid стола\tНомер\tКлиент\tЗаметки\tОткрыто\tОжидается\tЗакрыто";
        }

        public override string ToString()
        {
            var s = StartedDate.ToString("yyyy - MM - dd");
            var e = ExpectedDate == null ? string.Empty : ExpectedDate.Value.ToString("yyyy - MM - dd");
            var c = ClosedDate == null ? string.Empty : ClosedDate.Value.ToString("yyyy - MM - dd");
            return $"{Id}\t{ProductName}\t{StatusText}\t{TableId}\t{Number}\t{Client}\t{Details}\t{s}\t{e}\t{c}";
        }
    }
}
