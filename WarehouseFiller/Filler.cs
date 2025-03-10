using ExcelDataReader;
using System.Data.SQLite;
using System.Text;

namespace WarehouseFiller
{
    public class Filler : IDisposable
    {
        private FileStream _stream;
        private IExcelDataReader _reader;
        private List<Product> _products = [];
        private HashSet<string> _productNames = [];
        private List<ComponentType> _componentTypes = [];
        private List<Component> _components = [];
        private HashSet<string> _componentNames = [];
        private SQLiteConnection _connection;
        private bool _disposedValue;

        static Filler()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Fill(string pathToXlsx, string pathToDb)
        {
            ArgumentNullException.ThrowIfNull(pathToXlsx, nameof(pathToXlsx));
            ArgumentNullException.ThrowIfNull(pathToDb, nameof(pathToDb));

            OpenExcel(pathToXlsx);
            CheckExcel();
            CreateDatabase(pathToDb);
            FillGroups();
            FillComponents();
            FillProducts();
            FillProductComponents();
        }

        private void OpenExcel(string pathToXlsx)
        {
            _stream = File.Open(pathToXlsx, FileMode.Open, FileAccess.Read, FileShare.Read);
            _reader = ExcelReaderFactory.CreateReader(_stream);
        }

        private void CheckExcel()
        {
            if (_reader.RowCount < 100)
                throw new FillerException("Количество строк меньше 100");

            _reader.Read();
            if (_reader.FieldCount < 10)
                throw new FillerException("Количество столбцов меньше 10");

            CheckColumnName(0, "Наименование");
            CheckColumnName(1, "Цена");
            CheckColumnName(2, "Количество");

            GatherProducts();
            GatherComponents();

            _reader.Reset();
        }

        private void CheckColumnName(int index, string name)
        {
            var actualName = _reader.GetString(index);
            if (!actualName.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                throw new FillerException($"Столбец {name} не найден");
        }

        private void GatherProducts()
        {
            bool isUnit = true;
            int unitCount = 0;
            int tableCount = 0;
            for (int col = 3, id = 1; col < _reader.FieldCount; col++, id++)
            {
                var name = _reader.GetString(col);
                if (_productNames.Contains(name))
                    throw new FillerException($"Дублирующее название изделия: {name}");

                _productNames.Add(name);
                if (name.StartsWith("ПС.", StringComparison.InvariantCultureIgnoreCase))
                    isUnit = false;

                var product = new Product
                {
                    Id = id,
                    Name = name,
                    Column = col,
                    IsUnit = isUnit
                };
                _products.Add(product);

                if (isUnit)
                    unitCount++;
                else
                    tableCount++;
            }

            if (unitCount == 0)
                throw new FillerException("Узлы не найдены");
            if (tableCount == 0)
                throw new FillerException("Столы не найдены");
        }

        private void GatherComponents()
        {
            int row = 0;
            int componentTypeId = 0;
            int componentId = 0;
            int count = 0;
            bool first = true;
            bool hasUnits = false;
            bool isUnit = false;
            while (_reader.Read())
            {
                row++;
                var name = _reader.GetString(0);
                if (_reader.IsGroup())
                {
                    if (!first)
                    {
                        if (count == 0) // previous group was empty
                            throw new FillerException("Обнаружена пустая группа");
                    }

                    isUnit = string.Compare("узлы", name, StringComparison.InvariantCultureIgnoreCase) == 0;
                    if (isUnit)
                        hasUnits = true;
                    first = false;
                    count = 0;
                    _componentTypes.Add(new ComponentType { Id = ++componentTypeId, Name = name });
                }
                else
                {
                    if (componentTypeId == 0)
                        throw new FillerException("Не найдено название первой группы");

                    if (_componentNames.Contains(name))
                        throw new FillerException($"Дублирующее название комплектующего: {name}");
                    if (isUnit && !_productNames.Contains(name))
                        throw new FillerException($"Не найдено имя узла: {name}");

                    _componentNames.Add(name);
                    count++;
                    int? price = _reader.GetValue(1) == null ? null : (int)(_reader.GetDouble(1) * 100);
                    int amount = _reader.GetValue(2) == null ? 0 : (int)_reader.GetDouble(2);
                    var component = new Component
                    {
                        Id = ++componentId,
                        Name = name,
                        Type = componentTypeId,
                        Price = price,
                        Amount = amount,
                        IsUnit = isUnit
                    };
                    _components.Add(component);
                }
            }

            if (!hasUnits)
                throw new FillerException("Группа Узлы не найдена");
        }

        public void CreateDatabase(string filename)
        {
            // Create database from scratch
            if (File.Exists(filename))
                File.Delete(filename);

            _connection = new SQLiteConnection($"DataSource={filename};Mode=ReadWrite");
            _connection.Open();

            var query = @"
CREATE TABLE ComponentType (
    Id   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT    NOT NULL
)
STRICT;

CREATE TABLE Component (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT
                            NOT NULL,
    Name            TEXT    NOT NULL,
    Type            INTEGER NOT NULL
                            REFERENCES ComponentType (id),
    Amount          INTEGER NOT NULL
                            DEFAULT (0),
    AmountInUse     INTEGER NOT NULL
                            DEFAULT (0),
    Price           INTEGER,
    Ordered         INTEGER,
    ExpectedDate    TEXT,
    Details         TEXT,
    IsUnit          INTEGER NOT NULL CHECK (IsUnit IN (0, 1))
)
STRICT;

CREATE TABLE Product (
    Id      INTEGER PRIMARY KEY AUTOINCREMENT
                 NOT NULL,
    Name    TEXT    NOT NULL,
    IsUnit  INTEGER NOT NULL CHECK (IsUnit IN (0, 1))
)
STRICT;

CREATE TABLE ProductComponent (
    ProductId   INTEGER REFERENCES Product (id) 
                        NOT NULL,
    ComponentId INTEGER	REFERENCES Component (id) 
                        NOT NULL,
    Amount      INTEGER
)
STRICT;

CREATE TABLE Fabrication (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT
                            NOT NULL,
    ProductId       INTEGER REFERENCES Product (id),
    TableId         TEXT,
    Number          INTEGER,
    Status          INTEGER NOT NULL
                            DEFAULT (0),
    Client          TEXT,
    Details         TEXT,
    StartedDate     TEXT    NOT NULL
                            DEFAULT (DATE('now', 'localtime') ),
    ExpectedDate    TEXT,
    ClosedDate      TEXT
)
STRICT;
";
            using var command = new SQLiteCommand(query, _connection);
            command.ExecuteNonQuery();
        }

        private void FillGroups()
        {
            var query = new StringBuilder("INSERT INTO ComponentType (Id, Name) VALUES ");
            foreach (var group in _componentTypes)
            {
                query.Append($"({group.Id}, '{group.Name}'),");
            }
            query.Remove(query.Length - 1, 1);

            using var command = new SQLiteCommand(query.ToString(), _connection);
            command.ExecuteNonQuery();
        }

        private void FillComponents()
        {
            var query = new StringBuilder("INSERT INTO Component (Id, Name, Type, Amount, Price, IsUnit) VALUES ");
            foreach (var c in _components)
            {
                var price = c.Price == null ? "NULL" : c.Price.ToString();
                var isUnit = c.IsUnit ? "1" : "0";
                query.Append($"({c.Id}, '{c.Name}', {c.Type}, {c.Amount}, {price}, {isUnit}),");
            }
            query.Remove(query.Length - 1, 1);

            using var command = new SQLiteCommand(query.ToString(), _connection);
            command.ExecuteNonQuery();
        }

        private void FillProducts()
        {
            var query = new StringBuilder("INSERT INTO Product (Id, Name, IsUnit) VALUES ");
            foreach (var p in _products)
            {
                var isUnit = p.IsUnit ? "1" : "0";
                query.Append($"({p.Id}, '{p.Name}', {isUnit}),");
            }
            query.Remove(query.Length - 1, 1);

            using var command = new SQLiteCommand(query.ToString(), _connection);
            command.ExecuteNonQuery();
        }

        private void FillProductComponents()
        {
            _reader.Reset();
            _reader.Read();
            var query = new StringBuilder("INSERT INTO ProductComponent (ProductId, ComponentId, Amount) VALUES ");
            for (int componentId = 0; _reader.Read();)
            {
                if (_reader.IsGroup()) // skip group
                    continue;
                else
                    componentId++;

                foreach (var product in _products)
                {
                    var amount = _reader.GetIntValue(product.Column);
                    if (amount != null)
                        query.Append($"({product.Id}, {componentId}, {amount.Value}),");
                }
            }
            query.Remove(query.Length - 1, 1);

            using var command = new SQLiteCommand(query.ToString(), _connection);
            command.ExecuteNonQuery();
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
