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
            foreach (DataRow row in components.Table.Rows)
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

        public bool HasNegativeRemainders(int productId)
        {
            var query = @"
SELECT Remainder FROM
(
	SELECT Component.Amount - Component.AmountInUse AS Remainder
	FROM Component
	LEFT JOIN ProductComponent ON Id = ProductComponent.ComponentId WHERE ProductId = @productId
	ORDER BY Component.Amount - Component.AmountInUse
	LIMIT 1
)
";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            command.Parameters.Add(new SQLiteParameter("@productId", productId));
#if DEBUG
            WriteParameters(command);
#endif
            var value = command.ExecuteScalar();
            int result;
            if (value == null || value is DBNull)
                result = 0;
            else
                result = (int)(long)value;

            return result < 0;
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

        public int GetMaxFabricationNumber()
        {
            var query = @"
SELECT MAX(Number) AS MaxNumber
FROM Fabrication";
            System.Diagnostics.Debug.WriteLine(query);
            var command = new SQLiteCommand(query, _connection);
            var value = command.ExecuteScalar();
            int result;
            if (value == null || value is DBNull)
                result = 0;
            else
                result = (int)(long)value;
            return result;
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
    Fabrication.TableId,
    Fabrication.Number,
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
    Fabrication.TableId,
    Fabrication.Number,
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

        public void InsertFabrication(Fabrication fabrication)
        {
            var query = @"
INSERT INTO Fabrication (ProductId, Client, Details, TableId, Number, Status, StartedDate, ExpectedDate, ClosedDate)
VALUES
(@productId, @client, @details, @tableId, @number, @status, @startedDate, @expectedDate, @closedDate)
RETURNING id";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);
            command.Parameters.Add(new SQLiteParameter("@productId", fabrication.ProductId));
            command.Parameters.Add(new SQLiteParameter("@client", fabrication.Client));
            command.Parameters.Add(new SQLiteParameter("@details", fabrication.Details));
            command.Parameters.Add(new SQLiteParameter("@tableId", fabrication.TableId));
            command.Parameters.Add(new SQLiteParameter("@number", fabrication.Number));
            command.Parameters.Add(new SQLiteParameter("@status", fabrication.Status));
            command.Parameters.Add(new SQLiteParameter("@startedDate",
                fabrication.StartedDate.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new SQLiteParameter("@expectedDate", fabrication.ExpectedDate == null
                ? null
                : fabrication.ExpectedDate.Value.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new SQLiteParameter("@closedDate", fabrication.ClosedDate == null
                ? null
                : fabrication.ClosedDate.Value.ToString("yyyy-MM-dd")));
#if DEBUG
            WriteParameters(command);
#endif
            var value = (long)command.ExecuteScalar();
            int id = (int)value;
            fabrication.Id = id;
        }

        public void UpdateFabrication(Fabrication fabrication)
        {
            var query = @"
UPDATE Fabrication SET
    ProductId = @productId,
    Client = @client,
    Details = @details,
    TableId = @tableId,
    Number = @number,
    Status = @status,
    StartedDate = @startedDate,
    ExpectedDate = @expectedDate,
    ClosedDate = @closedDate
WHERE Id=@id";
            using var command = new SQLiteCommand(query, _connection);
            System.Diagnostics.Debug.WriteLine(query);
            command.Parameters.Add(new SQLiteParameter("@id", fabrication.Id));
            command.Parameters.Add(new SQLiteParameter("@productId", fabrication.ProductId));
            command.Parameters.Add(new SQLiteParameter("@client", fabrication.Client));
            command.Parameters.Add(new SQLiteParameter("@details", fabrication.Details));
            command.Parameters.Add(new SQLiteParameter("@tableId", fabrication.TableId));
            command.Parameters.Add(new SQLiteParameter("@number", fabrication.Number));
            command.Parameters.Add(new SQLiteParameter("@status", fabrication.Status));
            command.Parameters.Add(new SQLiteParameter("@startedDate",
                fabrication.StartedDate.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new SQLiteParameter("@expectedDate", fabrication.ExpectedDate == null
                ? null
                : fabrication.ExpectedDate.Value.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new SQLiteParameter("@closedDate", fabrication.ClosedDate == null
                ? null
                : fabrication.ClosedDate.Value.ToString("yyyy-MM-dd")));
#if DEBUG
            WriteParameters(command);
#endif
            command.ExecuteNonQuery();
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
