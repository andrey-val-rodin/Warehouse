using ExcelDataReader;
using System.Data.SQLite;
using WarehouseFiller;

namespace Tests
{
    public class DbInspector : IDisposable
    {
        private FileStream _stream;
        private IExcelDataReader _reader;
        private SQLiteConnection _connection;
        private bool _disposedValue;

        public void AssertDbValid(string pathToXlsx, string pathToDb)
        {
            OpenDb(pathToDb);
            OpenExcel(pathToXlsx);
            _reader.Read();

            for (int componentId = 0; _reader.Read();)
            {
                if (_reader.IsGroup()) // skip group
                    continue;
                else
                    componentId++;

                int expectedAmount = _reader.GetValue(2) == null ? 0 : _reader.GetIntValue(2).Value;
                int actualAmount = GetAmount(componentId);
                Assert.Equal(expectedAmount, actualAmount);

                for (int productId = 1, col = 3; col < _reader.FieldCount; productId++, col++)
                {
                    int? expectedRequiredCount = _reader.GetIntValue(col);
                    int? actualRequiredCount = GetRequiredCount(productId, componentId);
                    Assert.Equal(expectedRequiredCount, actualRequiredCount);
                }
            }
        }

        private void OpenExcel(string pathToXlsx)
        {
            _stream = File.Open(pathToXlsx, FileMode.Open, FileAccess.Read);
            _reader = ExcelReaderFactory.CreateReader(_stream);
        }

        private void OpenDb(string pathToDb)
        {
            _connection = new SQLiteConnection($"DataSource={pathToDb};Mode=Read");
            _connection.Open();
        }

        private int GetAmount(int componentId)
        {
            var query = "SELECT Amount FROM Component WHERE Id = @component";
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@component", componentId));
            var value = command.ExecuteScalar();
            return (int)(long)value;
        }

        private int? GetRequiredCount(int productId, int componentId)
        {
            var query = "SELECT Amount FROM ProductComponent WHERE ProductId = @product AND ComponentId = @component";
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@product", productId));
            command.Parameters.Add(new SQLiteParameter("@component", componentId));
            var value = command.ExecuteScalar();
            int? result;
            if (value == null || value is DBNull)
                result = null;
            else
                result = (int)(long)value;

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _reader?.Dispose();
                    _stream?.Dispose();
                    _connection?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
