using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Windows;
using Warehouse.Model;

namespace Warehouse.Database
{
    public class SqlProvider : ISqlProvider, IDisposable
    {
        private SQLiteConnection _connection;
        private bool disposedValue;

        public bool Connect(string path)
        {
            try
            {
                _connection = new SQLiteConnection($"DataSource={path};Mode=ReadWrite");
                _connection.Open();
            }
            catch (SQLiteException e)
            {
                MessageBox.Show(e.Message, "Ошибка открытия БД");
                return false;
            }

            return true;
        }

        public void Close()
        {
            _connection?.Close(); 
        }

        public string[] GetComponentTypes()
        {
            var query = "SELECT Name FROM ComponentType ORDER BY Id ASC";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);

            using var reader = command.ExecuteReader();
            var result = new List<string>();
            while (reader.Read())
            {
                if (reader.GetValue(0) is string name)
                    result.Add(name);
            }

            return [.. result];
        }

        public DataView GetComponents(int type)
        {
            var ds = new DataSet("Components");
            SQLiteCommand command;
            var builder = new StringBuilder(@"
SELECT
    Component.Id,
    Component.Type,
    Component.Name,
    Component.Amount,
    Component.Amount - Component.AmountInUse AS Remainder,
    CAST(Component.Price AS REAL)/100 AS Price,
    Ordered,
    Details
FROM Component");
            string query;
            if (type > 0)
            {
                builder.Append("\n LEFT JOIN ComponentType ON Type = ComponentType.Id WHERE Component.Type = @type");
                query = builder.ToString();
                command = new SQLiteCommand(query, _connection);
                System.Diagnostics.Debug.WriteLine(query);
                command.Parameters.Add(new SQLiteParameter("@type", type));
#if DEBUG
                WriteParameters(command);
#endif
            }
            else
            {
                query = builder.ToString();
                System.Diagnostics.Debug.WriteLine(query);
                command = new SQLiteCommand(query, _connection);
            }

            using var adapter = new SQLiteDataAdapter(command);
            adapter.Fill(ds);

            return ds.Tables[0].DefaultView;
        }

        public void UpdateComponent(Component component)
        {
            var query = @"
UPDATE Component SET
    Amount = @amount,
    Price = @price,
    Ordered = @ordered,
    Details = @details
WHERE Id=@id";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);
            command.Parameters.Add(new SQLiteParameter("@id", component.Id));
            command.Parameters.Add(new SQLiteParameter("@amount", component.Amount));
            command.Parameters.Add(new SQLiteParameter("@price", component.Price * 100));
            command.Parameters.Add(new SQLiteParameter("@ordered", component.Ordered));
            command.Parameters.Add(new SQLiteParameter("@details", component.Details));
#if DEBUG
            WriteParameters(command);
#endif
            command.ExecuteNonQuery();
        }

        public string[] GetProductNames()
        {
            var query = "SELECT Name FROM Product ORDER BY Id ASC";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);

            using var reader = command.ExecuteReader();
            var result = new List<string>();
            while (reader.Read())
            {
                if (reader.GetValue(0) is string name)
                    result.Add(name);
            }

            return [.. result];
        }

        public DataView GetProductComponents(int type)
        {
            var ds = new DataSet("ProductComponents");
            string query = @"
SELECT
    Component.Id,
    Component.Name,
    ProductComponent.Amount AS Required,
    Component.Amount,
    Component.Amount - Component.AmountInUse AS Remainder,
    CAST(Component.Price AS REAL)/100 AS Price,
    Ordered,
    Details
FROM Component
LEFT JOIN ProductComponent ON Id = ProductComponent.ComponentId WHERE ProductId = @product";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@product", type));
#if DEBUG
            WriteParameters(command);
#endif
            using var adapter = new SQLiteDataAdapter(command);
            adapter.Fill(ds);

            return ds.Tables[0].DefaultView;
        }

#if DEBUG
        private static void WriteParameters(SQLiteCommand command)
        {
            foreach (SQLiteParameter parameter in command.Parameters)
            {
                System.Diagnostics.Debug.WriteLine($"{parameter.ParameterName}={parameter.Value}");
            }
        }
#endif

        public decimal GetProductPrice(int type)
        {
            var query = @"
SELECT SUM(CAST(Component.Price AS REAL)/100) AS SumPrice
FROM Component
LEFT JOIN ProductComponent ON Id = ProductComponent.ComponentId WHERE ProductId = @product";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@product", type));
#if DEBUG
            WriteParameters(command);
#endif
            var result = (double)command.ExecuteScalar();
            return (decimal)result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }

                _connection = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
