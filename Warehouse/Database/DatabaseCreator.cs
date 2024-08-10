﻿using System.Data.SQLite;
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
                        NOT NULL,
    Amount      INTEGER
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
(34, 'Фланец.ПС.400-600.Фото.01 D40 L20',       3, 1,   NULL),
(35, 'Пруток D24 мм, шт. это 1 мм',             4, 2000,NULL),
(36, 'Пруток D40 мм, шт. это 1 мм',             4, 1060,NULL),
(37, 'Пруток D50 мм, шт. это 1 мм',             4, 500, NULL),
(38, 'Пруток D60 мм, шт. это 1 мм',             4, 760, NULL),
(39, 'Двигатель коллекторный 37GB31Y (260prm)',                         5, 10,  NULL),
(40, 'Блок питания.24в.36w',                                            5, 3,   NULL),
(41, 'Разъем питания C14 без пайки',                                    5, 4,   NULL),
(42, 'Регулятор ШИМ',                                                   5, 2,   NULL),
(43, 'Разъем DB-9F',                                                    5, 33,  NULL),
(44, 'Разъем DB-9M',                                                    5, 32,  NULL),
(45, 'Винт для разъема DB-9',                                           5, 5,   NULL),
(46, 'Корпус для DB-9M',                                                5, 21,  NULL),
(47, 'Винт для корпуса для DB-9M',                                      5, 54,  NULL),
(48, 'Корпус для РЭА G1068B (95х48х38) GAINTA',                         5, 7,   NULL),
(49, 'Кабельный ввод REXANT PG-7 (3.5-6 мм)',                           5, 18,  NULL),
(50, 'Переключатель KCD1-103-11-B/3P on-off-on ',                       5, 21,  NULL),
(51, 'Провод 6 pin 5 м',                                                5, 12,  NULL),
(52, 'Энкодер YN06-OP-36OB-2M',                                         5, 4,   NULL),
(53, 'Драйвер двигателя hw-039',                                        5, 4,   NULL),
(54, 'Плата 100х80',                                                    5, 3,   NULL),
(55, 'Контроллер ESP32u',                                               5, 3,   NULL),
(56, 'Переходник для антенны SMA-female u.fl/ipx',                      5, 5,   NULL),
(57, 'Антенна',                                                         5, 2,   NULL),
(58, 'Jack 3,5 TKX3-3.5-11 PCB',                                        5, 21,  NULL),
(59, 'Конденсатор 10 мкФ 50 в',                                         5, 25,  NULL),
(60, 'Преобраз. пониж. LM2596',                                         5, 9,   NULL),
(61, 'Оптопара PC817',                                                  5, 21,  NULL),
(62, 'Резистор 1 кОм 0,25 W',                                           5, 28,  NULL),
(63, 'Резистор 180 Ом 0,25 W',                                          5, 14,  NULL),
(64, 'Двухр. кабель Dupont 2х4 шаг 2,54 мм, l 110',                     5, 31,  NULL),
(65, 'Вилка штыревая PLD-40 по 2х4',                                    5, 140, NULL),
(66, 'Вилка штыревая PLS-40 по одной',                                  5, 1530,NULL),
(67, 'PBS-19 гнездо на плату',                                          5, 2,   NULL),
(68, 'Стойка печ. плат. мама-мама М3х12',                               5, 40,  NULL),
(69, 'Стойка  печ. плат. мама-мама М3х22',                              5, 104, NULL),
(70, 'Стойка  печ. плат. Мама-мама М3х25 (капрон)',                     5, 12,  NULL),
(71, 'Клеммник винтовой, 2-контактный, 5мм, прямой DG126-5.0-02P-14',   5, 6,   NULL),
(72, 'Клеммник, 2-контактный, 3.81мм, прямой   DG381-3.81-02P-14-00AH', 5, 43,  NULL),
(73, 'Кабель питания 220 вольт 1,8 м',                                  5, 2,   NULL),
(74, 'Ролик скейт 52-54 мм 95А',                6, 2,   NULL),
(75, 'Подшипник фланцевый F6201 2RR',           6, 16,  NULL),
(76, 'Подшипник 608 8х22х7 2RR',                6, 10,  NULL),
(77, 'Подшипник упорный 8101 12х26х9',          6, 16,  NULL),
(78, 'Подшипник упорный 35х52х12',              6, 40,  NULL),
(79, 'Стопорное кольцо наружное Д12 DIN471',    6, 200, NULL),
(80, 'Шкив 2GT.60 D8 mm 10 мм',                 6, 2,   NULL),
(81, 'Шкив 2GT.30 D6 mm 10 мм',                 6, 2,   NULL),
(82, 'Ремень 2GN 9 мм L210',                    6, 2,   NULL),
(83, 'Втулка распорная 10,3 мм',                6, 6,   NULL),
(84, 'Втулка внешняя 13 мм',                    6, 12,  NULL),
(85, 'Винт для самоката мама 55(56) мм',        6, 6,   NULL),
(86, 'Винт для самоката папа 12 мм',            6, 6,   NULL),
(87, 'Шкив 2GT.40 D8 7 mm',                     6, 13,  NULL),
(88, 'Шкив 2GT.40 D6 7 mm',                     6, 13,  NULL),
(89, 'Ремень 2GN 6 мм Z280',                    6, 8,   NULL),
(90, 'Натяжитель велосипедный',                 6, 11,  NULL),
(91, 'Заглушка Д20',                            6, 0,   NULL),
(92, 'Ножка Mi 9 (малая)',                      6, 1000,NULL),
(93, 'Ножка Mi 59A (Средняя)',                  6, 27,  NULL),
(94, 'Ножка (Большая)',                         6, 100, NULL),
(95, 'Гайка-заклепка М4',                       7, 85,  NULL),
(96, 'Гайка-заклепка М5',                       7, 10,  NULL),
(97, 'Гайка-заклепка М6',                       7, 300, NULL),
(98, 'Вытяжная заклепка 4х6',                   7, 600, NULL),
(99, 'Вытяжная заклепка 4х10',                 7, 0,   NULL),
(100, 'Вытяжная заклепка 4х14',                 7, 500, NULL),
(101, 'Винт потай М4х16 DIN 965',               8, 50,  NULL),
(102, 'Винт полусферический М3х6 DIN 7985',     8, 500, NULL),
(103, 'Винт полусферический М3х8 DIN 7985',     8, 0,   NULL),
(104, 'Винт полусферический М3х10 DIN 7985',    8, 78,  NULL),
(105, 'Винт полусферический М4х12 DIN 7985',    8, 0,   NULL),
(106, 'Винт полусферический М4х16А DIN 7985',   8, 0,   NULL),
(107, 'Винт полусферический М4х16Б DIN 7985',   8, 0,   NULL),
(108, 'Винт полусферический М5х16 DIN 7985',    8, 0,   NULL),
(109, 'Винт полусферический М5х20 DIN 7985',    8, 0,   NULL),
(110, 'Болт М6х14 DIN 933',                     8, 1000,NULL),
(111, 'Болт М6х100 DIN 933',                    8, 2,   NULL),
(112, 'Болт М10х25 DIN 933',                    8, 0,   NULL),
(113, 'Винт установочный М5х10',                8, 0,   NULL),
(114, 'Шайба М3 DIN 125A',                      9, 2000, NULL),
(115, 'Шайба М4 DIN 125A',                      9, 1000, NULL),
(116, 'Шайба М6 DIN 125A',                      9, 200, NULL),
(117, 'Шайба М8 DIN 125A',                      9, 200, NULL),
(118, 'Шайба М10 DIN 125A',                     9, 100, NULL),
(119, 'Шайба М12 DIN 125A',                     9, 0, NULL),
(120, 'Шайба увеличенная М4 DIN 9021',          9, 0, NULL),
(121, 'Гровер М3А DIN 127',                     9, 200, NULL),
(122, 'Гровер М3Б DIN 127',                     9, 300, NULL),
(123, 'Гровер М4А DIN 127',                     9, 66, NULL),
(124, 'Гровер М4Б DIN 127',                     9, 500, NULL),
(125, 'Гровер М5 DIN 127',                      9, 150, NULL),
(126, 'Гровер М6 DIN 127',                      9, 200, NULL),
(127, 'Гровер М10 DIN 127',                     9, 50, NULL),
(128, 'Шайба регулировочная 12х18х0,5',         9, 42, NULL),
(129, 'Шайба регулировочная 35х45х0,2',         9, 250, NULL),
(130, 'Шайба регулировочная 35х45х0,5 DIN 988', 9, 200, NULL),
(131, 'Гайка М3 DIN 934',   10, 40,     NULL),
(132, 'Гайка М4 DIN 934',   10, 1000,   NULL),
(133, 'Гайка М5 DIN 934',   10, 100,    NULL),
(134, 'Гайка М6 DIN 934',   10, 500,    NULL),
(135, 'Гайка М10 DIN 934',  10, 9,      NULL),
(136, 'Гайка М12х1',        10, 16,     NULL),
(137, 'Гайка М16х1',        10, 6,      NULL),
(138, 'Саморез прессшайба острый 19',    11, 400, NULL),
(139, 'Саморез прессшайба острый 25',    11, 100, NULL),
(140, 'Коробка 400',        12, 9, NULL),
(141, 'Коробка 600',        12, 9, NULL),
(142, 'Коробка 900',        12, 3, NULL),
(143, 'Кабельная стяжка',   12, 0, NULL),
(144, 'Стрейч, шт.',        12, 0, NULL),
(145, 'Пупырчатка, м',      12, 0, NULL),
(146, 'Скотч, шт.',         12, 0, NULL),
(147, 'Паспорт',            12, 0, NULL),
(148, 'Шильдик',            12, 0, NULL),
(149, 'Номер',              12, 0, NULL),
(150, 'Этикетка на коробку',12, 0, NULL);
";
            using var command = new SQLiteCommand(query, _connection);
            command.ExecuteNonQuery();
        }
    }
}
