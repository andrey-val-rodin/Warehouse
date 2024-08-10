using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows;

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
            var builder = new StringBuilder("SELECT Component.Id, Component.Type, Component.Name, Component.Amount, Component.Amount - Component.AmountInUse AS Remainder, CAST(Component.Price AS REAL)/100 AS Price FROM Component");
            string query;
            if (type > 0)
            {
                builder.Append(" LEFT JOIN ComponentType ON Type = ComponentType.Id WHERE Component.Type = @t");
                query = builder.ToString();
                command = new SQLiteCommand(query, _connection);
                System.Diagnostics.Debug.WriteLine(query);
                command.Parameters.Add(new SQLiteParameter("@t", type));
                System.Diagnostics.Debug.WriteLine($"@t={type}");
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

        public void UpdateComponentAmount(int componentId, int amount)
        {
            var query = "UPDATE Component SET Amount = @value WHERE Id=@id";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);
            command.Parameters.Add(new SQLiteParameter("@id", componentId));
            command.Parameters.Add(new SQLiteParameter("@value", amount));
            System.Diagnostics.Debug.WriteLine($"@id={componentId}");
            System.Diagnostics.Debug.WriteLine($"@value={amount}");
            command.ExecuteNonQuery();
        }

        public void UpdateComponentAmountInUse(int componentId, int amountInUse)
        {
            var query = "UPDATE Component SET AmountInUse = @value WHERE Id=@id";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);
            command.Parameters.Add(new SQLiteParameter("@id", componentId));
            command.Parameters.Add(new SQLiteParameter("@value", amountInUse));
            System.Diagnostics.Debug.WriteLine($"@id={componentId}");
            System.Diagnostics.Debug.WriteLine($"@value={amountInUse}");
            command.ExecuteNonQuery();
        }

        public void UpdateComponentPrice(int componentId, decimal? price)
        {
            var query = "UPDATE Component SET Price = @value WHERE Id=@id";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);
            command.Parameters.Add(new SQLiteParameter("@id", componentId));
            command.Parameters.Add(new SQLiteParameter("@value", price));
            System.Diagnostics.Debug.WriteLine($"@id={componentId}");
            System.Diagnostics.Debug.WriteLine($"@value={price}");
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

        public DataView GetProductComponents(int type)
        {
            var ds = new DataSet("ProductComponents");
            string query = @"
SELECT Component.Id, Component.Name, ProductComponent.Amount AS Required, Component.Amount, Component.Amount - Component.AmountInUse AS Remainder, CAST(Component.Price AS REAL)/100 AS Price
FROM Component
LEFT JOIN ProductComponent ON Id = ProductComponent.ComponentId WHERE ProductId = @p";
                System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@p", type));
            System.Diagnostics.Debug.WriteLine($"@p={type}");
            using var adapter = new SQLiteDataAdapter(command);
            adapter.Fill(ds);

            return ds.Tables[0].DefaultView;
        }
    }
}
