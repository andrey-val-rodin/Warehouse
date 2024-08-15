﻿using System.Data;
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

        private DataView GetComponentsDataView(int typeId)
        {
            var ds = new DataSet("Components");
            SQLiteCommand command;
            var builder = new StringBuilder(@"
SELECT
    Component.Id,
    Component.Type,
    Component.Name,
    Component.Amount,
    Component.AmountInUse,
    Component.Amount - Component.AmountInUse AS Remainder,
    CAST(Component.Price AS REAL)/100 AS Price,
    Ordered,
    ExpectedDate,
    Details
FROM Component");
            string query;
            if (typeId > 0)
            {
                builder.Append("\n LEFT JOIN ComponentType ON Type = ComponentType.Id WHERE Component.Type = @type");
                query = builder.ToString();
                command = new SQLiteCommand(query, _connection);
                System.Diagnostics.Debug.WriteLine(query);
                command.Parameters.Add(new SQLiteParameter("@type", typeId));
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
        
        public IEnumerable<Component> GetComponents(int typeId)
        {
            DataView components = GetComponentsDataView(typeId);
            foreach(DataRow row in components.Table.Rows)
            {
                yield return Component.FromDataRow(row);
            }
        }

        public void UpdateComponent(Component component)
        {
            var query = @"
UPDATE Component SET
    Amount = @amount,
    AmountInUse = @amountInUse,
    Price = @price,
    Ordered = @ordered,
    ExpectedDate = @expectedDate,
    Details = @details
WHERE Id=@id";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);
            command.Parameters.Add(new SQLiteParameter("@id", component.Id));
            command.Parameters.Add(new SQLiteParameter("@amount", component.Amount));
            command.Parameters.Add(new SQLiteParameter("@amountInUse", component.AmountInUse));
            command.Parameters.Add(new SQLiteParameter("@price", component.Price * 100));
            command.Parameters.Add(new SQLiteParameter("@ordered", component.Ordered));
            command.Parameters.Add(new SQLiteParameter("@expectedDate", component.ExpectedDate == null
                ? null
                : component.ExpectedDate.Value.ToString("yyyy-MM-dd")));
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

        private DataView GetProductComponentsDataView(int productId)
        {
            var ds = new DataSet("ProductComponents");
            string query = @"
SELECT
    Component.Id,
    Component.Name,
    ProductComponent.Amount AS Required,
    Component.Amount,
    Component.AmountInUse,
    Component.Amount - Component.AmountInUse AS Remainder,
    CAST(Component.Price AS REAL)/100 AS Price,
    Ordered,
    ExpectedDate,
    Details
FROM Component
LEFT JOIN ProductComponent ON Id = ProductComponent.ComponentId WHERE ProductId = @product";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@product", productId));
#if DEBUG
            WriteParameters(command);
#endif
            using var adapter = new SQLiteDataAdapter(command);
            adapter.Fill(ds);

            return ds.Tables[0].DefaultView;
        }

        public IEnumerable<ProductComponent> GetProductComponents(int productId)
        {
            DataView components = GetProductComponentsDataView(productId);
            foreach (DataRow row in components.Table.Rows)
            {
                yield return ProductComponent.FromDataRow(row);
            }
        }

        public decimal GetProductPrice(int productId)
        {
            var query = @"
SELECT SUM(CAST(Component.Price AS REAL)/100) AS SumPrice
FROM Component
LEFT JOIN ProductComponent ON Id = ProductComponent.ComponentId WHERE ProductId = @product";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@product", productId));
#if DEBUG
            WriteParameters(command);
#endif
            var value = command.ExecuteScalar();
            decimal result;
            if (value == null || value is DBNull)
                result = 0M;
            else
                result = (decimal)(double)value;
            return result;
        }

        public void AddProductAmountsInUse(int productId)
        {
            var query = @"
UPDATE Component
SET AmountInUse = (SELECT Component.AmountInUse + pc.Amount
					FROM ProductComponent pc
					WHERE Component.Id = pc.ComponentId AND pc.ProductId = @productId)
WHERE EXISTS (SELECT pc.Amount
					FROM ProductComponent pc
					WHERE Component.Id = pc.ComponentId AND pc.ProductId = @productId)
";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@productId", productId));
#if DEBUG
            WriteParameters(command);
#endif
            command.ExecuteNonQuery();
        }

        public void SubtractProductAmountsInUse(int productId)
        {
            var query = @"
UPDATE Component
SET AmountInUse = (SELECT Component.AmountInUse - pc.Amount
					FROM ProductComponent pc
					WHERE Component.Id = pc.ComponentId AND pc.ProductId = @productId)
WHERE EXISTS (SELECT pc.Amount
					FROM ProductComponent pc
					WHERE Component.Id = pc.ComponentId AND pc.ProductId = @productId)
";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@productId", productId));
#if DEBUG
            WriteParameters(command);
#endif
            command.ExecuteNonQuery();
        }

        public void SubtractProductAmounts(int productId)
        {
            var query = @"
UPDATE Component
SET Amount = (SELECT Component.Amount - pc.Amount
					FROM ProductComponent pc
					WHERE Component.Id = pc.ComponentId AND pc.ProductId = @productId)
WHERE EXISTS (SELECT pc.Amount
					FROM ProductComponent pc
					WHERE Component.Id = pc.ComponentId AND pc.ProductId = @productId)
";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@productId", productId));
#if DEBUG
            WriteParameters(command);
#endif
            command.ExecuteNonQuery();
        }

        private DataView GetOpenedFabricationsDataView()
        {
            var ds = new DataSet("Fabrications");
            string query = @"
SELECT
    Fabrication.Id,
    Fabrication.ProductId,
    Product.Name AS ProductName,
    Fabrication.Client,
    Fabrication.Details,
    Fabrication.Status,
    Fabrication.StartedDate,
    Fabrication.ExpectedDate,
    Fabrication.ClosedDate
FROM Fabrication
LEFT JOIN Product ON Fabrication.ProductId = Product.Id
WHERE Fabrication.Status = 0
ORDER BY Fabrication.Id ASC";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            using var adapter = new SQLiteDataAdapter(command);
            adapter.Fill(ds);

            return ds.Tables[0].DefaultView;
        }

        private DataView GetHistoricalFabricationsDataView()
        {
            var ds = new DataSet("Fabrications");
            string query = @"
SELECT
    Fabrication.Id,
    Fabrication.ProductId,
    Product.Name AS ProductName,
    Fabrication.Client,
    Fabrication.Details,
    Fabrication.Status,
    Fabrication.StartedDate,
    Fabrication.ExpectedDate,
    Fabrication.ClosedDate
FROM Fabrication
LEFT JOIN Product ON Fabrication.ProductId = Product.Id
WHERE Fabrication.Status <> 0
ORDER BY Fabrication.Id ASC";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            using var adapter = new SQLiteDataAdapter(command);
            adapter.Fill(ds);

            return ds.Tables[0].DefaultView;
        }

        public IEnumerable<Fabrication> GetOpenedFabrications()
        {
            DataView fabrications = GetOpenedFabricationsDataView();
            foreach (DataRow row in fabrications.Table.Rows)
            {
                yield return Fabrication.FromDataRow(row);
            }
        }

        public IEnumerable<Fabrication> GetHistoricalFabrications()
        {
            DataView fabrications = GetHistoricalFabricationsDataView();
            foreach (DataRow row in fabrications.Table.Rows)
            {
                yield return Fabrication.FromDataRow(row);
            }
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
