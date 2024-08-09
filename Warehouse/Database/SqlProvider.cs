using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Windows;

namespace Warehouse.Database
{
    public class SqlProvider : ISqlProvider
    {
        //TODO: temporary path
        private readonly SQLiteConnection _connection = new SQLiteConnection("DataSource=C:\\Users\\andre\\OneDrive\\Desktop\\Components.db;Mode=ReadWrite");

        public bool Connect()
        {
            try
            {
                _connection.Open();
            }
            catch (SQLiteException e)
            {
                MessageBox.Show(e.Message, "Ошибка открытия БД");
                return false;
            }

            return true;
        }

        public string[] GetComponentTypes()
        {
            using var command = new SQLiteCommand("SELECT Name FROM ComponentType ORDER BY Id ASC", _connection);
            using var reader = command.ExecuteReader();
            var result = new List<string>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    if (reader.GetValue(0) is string name)
                        result.Add(name);
                }
            }

            return [.. result];
        }

        public DataView GetComponents(int type)
        {
            var ds = new DataSet("Components");
            SQLiteCommand command;
            var text = new StringBuilder("SELECT Component.Id, Component.Type, Component.Name, Component.Amount, Component.Amount - Component.AmountInUse AS Remainder, CAST(Component.Price AS REAL)/100 AS Price FROM Component");
            if (type > 0)
            {
                text.Append(" LEFT JOIN ComponentType ON Type = ComponentType.Id WHERE Component.Type = @t");
                command = new SQLiteCommand(text.ToString(), _connection);
                command.Parameters.Add(new SQLiteParameter("@t", type));
            }
            else
                command = new SQLiteCommand(text.ToString(), _connection);

            using var adapter = new SQLiteDataAdapter(command);
            adapter.Fill(ds);

            return ds.Tables[0].DefaultView;
        }

        public void UpdateComponentAmount(int componentId, int amount)
        {
            using var command = new SQLiteCommand("UPDATE Component SET Amount = @value WHERE Id=@id", _connection);
            command.Parameters.Add(new SQLiteParameter("@id", componentId));
            command.Parameters.Add(new SQLiteParameter("@value", amount));
            command.ExecuteNonQuery();
        }

        public void UpdateComponentAmountInUse(int componentId, int amountInUse)
        {
            using var command = new SQLiteCommand("UPDATE Component SET AmountInUse = @value WHERE Id=@id", _connection);
            command.Parameters.Add(new SQLiteParameter("@id", componentId));
            command.Parameters.Add(new SQLiteParameter("@value", amountInUse));
            command.ExecuteNonQuery();
        }

        public void UpdateComponentPrice(int componentId, decimal? price)
        {
            using var command = new SQLiteCommand("UPDATE Component SET Price = @value WHERE Id=@id", _connection);
            command.Parameters.Add(new SQLiteParameter("@id", componentId));
            command.Parameters.Add(new SQLiteParameter("@value", price));
            command.ExecuteNonQuery();
        }
    }
}
