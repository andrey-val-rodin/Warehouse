using System.Data.SQLite;
using System.IO;

namespace Warehouse.Database
{
    public class DatabaseCreator
    {
        private SQLiteConnection _connection;

        public void CreateDatabase(string filename)
        {
            // Create database from scratch
            if (File.Exists(filename))
                File.Delete(filename);

            _connection = new SQLiteConnection($"DataSource={filename};Mode=ReadWrite");
            _connection.Open();

            CreateTables();
            FillComponentType();
            FillComponent();
            _connection.Close();
        }

        private void CreateTables()
        {
            var query = @"
CREATE TABLE ComponentType (
    Id   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT    NOT NULL
)
STRICT;

CREATE TABLE Component (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT
                        NOT NULL,
    Name        TEXT    NOT NULL,
    Type        INTEGER NOT NULL
                        REFERENCES ComponentType (id),
    Amount      INTEGER NOT NULL
                        DEFAULT (0),
    AmountInUse INTEGER NOT NULL
                        DEFAULT (0),
    Price       INTEGER
)
STRICT;

CREATE TABLE Product (
    Id   INTEGER PRIMARY KEY AUTOINCREMENT
                 NOT NULL,
    Name TEXT    NOT NULL
)
STRICT;

CREATE TABLE ProductComponent (
    ProductId   INTEGER REFERENCES Product (id) 
                        NOT NULL,
    ComponentId INTEGER	REFERENCES Component (id) 
                        NOT NULL
)
STRICT;

CREATE TABLE Fabrication (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT
                      NOT NULL,
    ProductId INTEGER REFERENCES Product (id),
    Client    TEXT,
    Details   TEXT,
    TableId   TEXT,
    Status    INTEGER NOT NULL
                      DEFAULT (0),
    Started   TEXT    NOT NULL
                      DEFAULT (DATETIME('now') ),
    Closed    TEXT
)
STRICT;

CREATE TABLE Booking (
    Id          INTEGER PRIMARY KEY ASC AUTOINCREMENT
                        NOT NULL,
    ComponentId INTEGER REFERENCES Component (id) 
                        NOT NULL,
    Status      INTEGER NOT NULL
                        DEFAULT (0),
    Price       INTEGER NOT NULL
                        DEFAULT (0),
    Details     TEXT,
    Amount      INTEGER NOT NULL
                        DEFAULT (1),
    Created     TEXT    NOT NULL
                        DEFAULT (DATETIME('now', 'localtime') ),
    Expected    TEXT    NOT NULL
                        DEFAULT (DATETIME('now', 'localtime') ),
    Closed      TEXT
)
STRICT;
";
            using var command = new SQLiteCommand(query, _connection);
            command.ExecuteNonQuery();
        }

        private void FillComponentType()
        {
            var query = @"
INSERT INTO ComponentType (Id, Name)
VALUES
(1, 'Столешницы'),
(2, 'Детали стальные гнутые'),
(3, 'Токарка'),
(4, 'Заготовки'),
(5, 'Покупные изделия электрика'),
(6, 'Покупные изделия механика'),
(7, 'Метизы'),
(8, 'Болты'),
(9, 'Шайбы'),
(10, 'Гайки'),
(11, 'Саморезы'),
(12, 'Другое');
";
            using var command = new SQLiteCommand(query, _connection);
            command.ExecuteNonQuery();
        }

        private void FillComponent()
        {
            var query = @"
INSERT INTO Component (Id, Name, Type, Amount, Price)
VALUES
(1, 'Столешница.ПС.400  (МДФ 18 мм)',           1, 5,   NULL),
(2, 'Столешница.ПС.600  (МДФ 22 мм)',           1, 0,   NULL),
(3, 'Столешница.ПС.900  (МДФ 25 мм)',           1, 0,   NULL),
(4, 'Столешница.ПС.1200  (МДФ 25 мм)',          1, 0,   NULL),
(5, 'Столешница.ПС.1500  (МДФ 30 мм)',          1, 0,   NULL),
(6, 'Комплект.400',                             2, 0,   NULL),
(7, 'Комплект.600',                             2, 3,   NULL),
(8, 'Комплект.900',                             2, 0,   NULL),
(9, 'Комплект.1200',                            2, 0,   NULL),
(10, 'Комплект.1500',                           2, 0,   NULL),
(11, 'Пластина Фланца.ПС.400-600.01',           2, 3,   NULL),
(12, 'Ограничитель подшипника.ПС.400-600.01',   2, 0,   NULL),
(13, 'Скоба поворота.ПС.400-600.01',            2, 3,   NULL),
(14, 'Пластина столешницы.ПС.400-600.01',       2, 3,   NULL),
(15, 'Скоба ролика ведущего.ПС.400-600.01',     2, 3,   NULL),
(16, 'Скоба ролика ведомого.ПС.400-600.01',     2, 3,   NULL),
(17, 'Скоба двигателя.ПС.01',                   2, 3,   NULL),
(18, 'Ограничитель подшипника.ПС.900-1500.01',  2, 40,  NULL),
(19, 'Ограничитель столешницы.ПС.1200-1500.01', 2, 0,   NULL),
(20, 'Опора столешницы.ПС.1200-1500.01',        2, 0,   NULL),
(21, 'Скоба энкодера.ПС.900-1500.01',           2, 0,   NULL),
(22, 'Панель фото.ПС.01',                       2, 22,  NULL),
(23, 'Панель видео.ПС.01',                      2, 22,  NULL),
(24, 'Панель пульт.ПС.01',                      2, 21,  NULL),
(25, 'Панель корпус.ПС.01',                     2, 16,  NULL),
(26, 'Пластина логотипа.ПС.900-1500.01',        2, 7,   NULL),
(27, 'Пластина логотипа.ПС.400-600.01',         2, 10,  NULL),
(28, 'Вал центральный.ПС.3 болта.01 D60 L60',   3, 0,   NULL),
(29, 'Вал центральный.ПС.4 болта.01 D60 L60',   3, 2,   NULL),
(30, 'Вал шкива.ПС.02 D50 L40',                 3, 3,   NULL),
(31, 'Втулка ролика ведущего.ПС.01 D24 L20',    3, 0,   NULL),
(32, 'Вал ролика ведущего.ПС.01 D24 L120',      3, 0,   NULL),
(33, 'Фланец.ПС.400-600.Видео.01 D40 L60',      3, 1,   NULL),
(34, 'Фланец.ПС.400-600.Фото.01 D40 L20',       3, 1,   NULL);
";
            using var command = new SQLiteCommand(query, _connection);
            command.ExecuteNonQuery();
        }
    }
}
