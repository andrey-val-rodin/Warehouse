﻿using System.Data.SQLite;

namespace Tests
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
            FillProduct();
            FillProductComponent();
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
(1,		'Столешница.ПС.400  (МДФ 18 мм)',									1,	5,		 NULL),
(2,		'Столешница.ПС.600  (МДФ 22 мм)',									1,	0,		 NULL),
(3,		'Столешница.ПС.900  (МДФ 25 мм)',									1,	0,		 NULL),
(4,		'Столешница.ПС.1200  (МДФ 25 мм)',									1,	0,		 NULL),
(5,		'Столешница.ПС.1500  (МДФ 30 мм)',									1,	0,		 NULL),
(6,		'Комплект.400',														2,	0,		 NULL),
(7,		'Комплект.600',														2,	3,		 NULL),
(8,		'Комплект.900',														2,	0,		 NULL),
(9,		'Комплект.1200',													2,	0,		 NULL),
(10,	'Комплект.1500',													2,	0,		 NULL),
(11,	'Пластина Фланца.ПС.400-600.01',									2,	3,		 NULL),
(12,	'Ограничитель подшипника.ПС.400-600.01',							2,	0,		 NULL),
(13,	'Скоба поворота.ПС.400-600.01',										2,	3,		 NULL),
(14,	'Пластина столешницы.ПС.400-600.01',								2,	3,		 NULL),
(15,	'Скоба ролика ведущего.ПС.400-600.01',								2,	3,		 NULL),
(16,	'Скоба ролика ведомого.ПС.400-600.01',								2,	3,		 NULL),
(17,	'Скоба двигателя.ПС.01',											2,	3,		 NULL),
(18,	'Ограничитель подшипника.ПС.900-1500.01',							2,	40,		 NULL),
(19,	'Ограничитель столешницы.ПС.1200-1500.01',							2,	0,		 NULL),
(20,	'Опора столешницы.ПС.1200-1500.01',									2,	0,		 NULL),
(21,	'Скоба энкодера.ПС.900-1500.01',									2,	0,		 NULL),
(22,	'Панель фото.ПС.01',												2,	22,		 NULL),
(23,	'Панель видео.ПС.01',												2,	22,		 NULL),
(24,	'Панель пульт.ПС.01',												2,	21,		 NULL),
(25,	'Панель корпус.ПС.01',												2,	16,		 NULL),
(26,	'Пластина логотипа.ПС.900-1500.01',									2,	7,		 NULL),
(27,	'Пластина логотипа.ПС.400-600.01',									2,	10,		 NULL),
(28,	'Вал центральный.ПС.3 болта.01 D60 L60',							3,	0,		 NULL),
(29,	'Вал центральный.ПС.4 болта.01 D60 L60',							3,	2,		 NULL),
(30,	'Вал шкива.ПС.02 D50 L40',											3,	3,		 NULL),
(31,	'Втулка ролика ведущего.ПС.01 D24 L20',								3,	0,		 NULL),
(32,	'Вал ролика ведущего.ПС.01 D24 L120',								3,	0,		 NULL),
(33,	'Фланец.ПС.400-600.Видео.01 D40 L60',								3,	1,		 NULL),
(34,	'Фланец.ПС.400-600.Фото.01 D40 L20',								3,	1,		 NULL),
(35,    'Пруток D24 мм, шт. это 1 мм',                                      4,  2000,    NULL),
(36,	'Пруток D40 мм,  шт. это 1 мм',										4,	1060,	 NULL),
(37,	'Пруток D50 мм,  шт. это 1 мм',										4,	500,	 NULL),
(38,	'Пруток D60 мм,  шт. это 1 мм',										4,	760,	 NULL),
(39,	'Двигатель коллекторный 37GB31Y (260prm)',							5,	10,		 NULL),
(40,    'Блок питания.24в.36w',                                             5,  3,       NULL),
(41,	'Разъем питания C14 без пайки',										5,	9,		 NULL),
(42,	'Регулятор ШИМ',													5,	2,		 NULL),
(43,	'Разъем DB-9F',														5,	33,		 NULL),
(44,	'Разъем DB-9M',														5,	32,		 NULL),
(45,	'Винт для разъема DB-9',											5,	5,		 NULL),
(46,	'Корпус для DB-9M',													5,	21,		 NULL),
(47,	'Винт для корпуса для DB-9M',										5,	54,		 NULL),
(48,	'Корпус для РЭА G1068B (95х48х38) GAINTA',							5,	7,		 NULL),
(49,	'Кабельный ввод REXANT PG-7 (3.5-6 мм)',							5,	18,		 NULL),
(50,	'Переключатель KCD1-103-11-B/3P on-off-on',							5,	21,		 NULL),
(51,	'Провод 6 pin 5 м',													5,	12,		 NULL),
(52,	'Энкодер YN06-OP-36OB-2M',											5,	4,		 NULL),
(53,	'Драйвер двигателя hw-039',											5,	4,		 NULL),
(54,	'Плата 100х80',														5,	3,		 NULL),
(55,	'Контроллер ESP32u',												5,	3,		 NULL),
(56,	'Переходник для антенны SMA-female u.fl/ipx',						5,	5,		 NULL),
(57,	'Антенна',															5,	2,		 NULL),
(58,	'Jack 3,5 TKX3-3.5-11 PCB',											5,	21,		 NULL),
(59,	'Конденсатор 10 мкФ 50 в',											5,	25,		 NULL),
(60,	'Преобраз. пониж. LM2596',											5,	9,		 NULL),
(61,	'Оптопара PC817',													5,	21,		 NULL),
(62,	'Резистор 1 кОм 0,25 W',											5,	28,		 NULL),
(63,	'Резисттор 180 Ом 0,25 W',											5,	14,		 NULL),
(64,	'Двухр. кабель Dupont 2х4 шаг 2,54 мм, l 110',						5,	31,		 NULL),
(65,	'Вилка штыревая PLD-40 по 2х4',										5,	140,	 NULL),
(66,	'Вилка штыревая PLS-40 по одной',									5,	1530,	 NULL),
(67,	'PBS-19 гнездо на плату',											5,	2,		 NULL),
(68,	'Стойка печ. плат. мама-мама М3х12',								5,	40,		 NULL),
(69,	'Стойка  печ. плат. мама-мама М3х22',								5,	104,	 NULL),
(70,	'Стойка  печ. плат. Мама-мама М3х25 (капрон)',						5,	12,		 NULL),
(71,	'Клеммник винтовой, 2-контактный, 5мм, прямой DG126-5.0-02P-14',	5,	6,		 NULL),
(72,	'Клеммник, 2-контактный, 3.81мм, прямой   DG381-3.81-02P-14-00AH',	5,	43,		 NULL),
(73,	'Кабель питания 220 вольт 1,8 м',									5,	2,		 NULL),
(74,	'Ролик скейт 52-54 мм 95А',											6,	18,		 NULL),
(75,	'Подшипник фланцевый F6201 2RR',									6,	16,		 NULL),
(76,	'Подшипник 608 8х22х7 2RR',											6,	10,		 NULL),
(77,	'Подшипник упорный 8101 12х26х9',									6,	16,		 NULL),
(78,	'Подшипник упорный 8107 35х52х12',									6,	40,		 NULL),
(79,	'Стопорное кольцо наружное Д12 DIN471',								6,	200,	 NULL),
(80,	'Шкив 2GT.60 D8 mm 10 мм',											6,	2,		 NULL),
(81,	'Шкив 2GT.30 D6 mm 10 мм',											6,	2,		 NULL),
(82,	'Ремень 2GN 9 мм L210',												6,	2,		 NULL),
(83,	'Втулка распорная 10,3 мм',											6,	6,		 NULL),
(84,	'Втулка внешняя 13 мм',												6,	12,		 NULL),
(85,	'Винт для самоката мама 55(56) мм',									6,	6,		 NULL),
(86,	'Винт для самоката папа 12 мм',										6,	6,		 NULL),
(87,	'Шкив 2GT.40 D8 7 mm',												6,	13,		 NULL),
(88,	'Шкив 2GT.40 D6 7 mm',												6,	13,		 NULL),
(89,	'Ремень 2GN 6 мм Z280',												6,	8,		 NULL),
(90,	'Натяжитель велосипедный',											6,	11,		 NULL),
(91,	'Заглушка Д20',														6,	0,		 NULL),
(92,	'Ножка Mi 9 (малая)',												6,	1000,	 NULL),
(93,	'Ножка Mi 59A (Средняя)',											6,	27,		 NULL),
(94,	'Ножка (Большая)',													6,	100,	 NULL),
(95,	'Гайка-заклепка М4',												7,	85,		 NULL),
(96,	'Гайка-заклепка М5',												7,	10,		 NULL),
(97,	'Гайка-заклепка М6',												7,	300,	 NULL),
(98,	'Вытяжная заклепка 4х6',											7,	600,	 NULL),
(99,	'Вытяжная заклепка 4х10',											7,	0,		 NULL),
(100,	'Вытяжная заклепка 4х14',											7,	500,	 NULL),
(101,	'Винт потай М4х16 DIN 965',											8,	50,		 NULL),
(102,	'Винт полусферический М3х6 DIN 7985',								8,	500,	 NULL),
(103,	'Винт полусферический М3х8 DIN 7985',								8,	0,		 NULL),
(104,	'Винт полусферический М3х10 DIN 7985',								8,	78,		 NULL),
(105,	'Винт полусферический М4х12 DIN 7985',								8,	0,		 NULL),
(106,	'Винт полусферический М4х16А DIN 7985',								8,	0,		 NULL),
(107,	'Винт полусферический М4х16Б DIN 7985',								8,	0,		 NULL),
(108,	'Винт полусферический М5х20 DIN 7985',								8,	0,		 NULL),
(109,	'Болт М6х14 DIN 933',												8,	1000,	 NULL),
(110,	'Болт М6х100 DIN 933',												8,	2,		 NULL),
(111,	'Болт М10х25 DIN 933',												8,	0,		 NULL),
(112,	'Винт установочный М5х10',											8,	0,		 NULL),
(113,	'Шайба М3 DIN 125A',												9,	2000,	 NULL),
(114,	'Шайба М4 DIN 125A',												9,	1000,	 NULL),
(115,	'Шайба М6 DIN 125A',												9,	200,	 NULL),
(116,	'Шайба М8 DIN 125A',												9,	200,	 NULL),
(117,	'Шайба М10 DIN 125A',												9,	100,	 NULL),
(118,	'Шайба М12 DIN 125A',												9,	0,		 NULL),
(119,	'Шайба увеличенная М4 DIN 9021',									9,	0,		 NULL),
(120,	'Гровер М3А DIN 127',												9,	200,	 NULL),
(121,	'Гровер М3Б DIN 127',												9,	300,	 NULL),
(122,	'Гровер М4А DIN 127',												9,	66,		 NULL),
(123,	'Гровер М4Б DIN 127',												9,	500,	 NULL),
(124,	'Гровер М5 DIN 127',												9,	150,	 NULL),
(125,	'Гровер М6 DIN 127',												9,	200,	 NULL),
(126,	'Гровер М10 DIN 127',												9,	50,		 NULL),
(127,	'Шайба регулировочная 12х18х0,5',									9,	42,		 NULL),
(128,	'Шайба регулировочная 35х45х0,2 ',									9,	250,	 NULL),
(129,	'Шайба регулировочная 35х45х0,5 DIN 988',							9,	200,	 NULL),
(130,	'Гайка М3 DIN 934',													10,	40,		 NULL),
(131,	'Гайка М4 DIN 934',													10,	1000,	 NULL),
(132,	'Гайка М5 DIN 934',													10,	100,	 NULL),
(133,	'Гайка М6 DIN 934',													10,	500,	 NULL),
(134,	'Гайка М10 DIN 934',												10,	9,		 NULL),
(135,	'Гайка М12х1 DIN 439',												10,	16,		 NULL),
(136,	'Гайка М16х1 DIN 439',												10,	6,		 NULL),
(137,	'Саморез прессшайба острый 19 мм',									11,	400,	 NULL),
(138,	'Саморез прессшайба острый 25 мм',									11,	100,	 NULL),
(139,	'Коробка 400',														12,	9,		 NULL),
(140,	'Коробка 600',														12,	9,		 NULL),
(141,	'Коробка 900',														12,	3,		 NULL),
(142,	'Кабельная стяжка',													12,	0,		 NULL),
(143,	'Стрейч, м',														12,	180,	 NULL),
(144,	'Пупырчатка, м',													12,	0,		 NULL),
(145,	'Скотч, м',															12,	70,		 NULL),
(146,	'Паспорт фото',														12,	0,		 NULL),
(147,	'Паспорт видео',													12,	0,		 NULL),
(148,	'Паспорт пульт/корпус',												12,	0,		 NULL),
(149,	'Шильдик',															12,	0,		 NULL),
(150,	'Номер',															12,	0,		 NULL),
(151,	'Этикетка на коробку',												12,	0,		 NULL);
";
            using var command = new SQLiteCommand(query, _connection);
            command.ExecuteNonQuery();
        }

        private void FillProduct()
        {
            var query = @"
INSERT INTO Product (Id, Name)
VALUES
(1, 'ПС.400.Фото'),
(2, 'ПС.400.Видео'),
(3, 'ПС.400.Пульт'),
(4, 'ПС.400.Корпус'),
(5, 'ПС.600.Фото'),
(6, 'ПС.600.Видео'),
(7, 'ПС.600.Пульт'),
(8, 'ПС.600.Корпус'),
(9, 'ПС.900.Фото'),
(10, 'ПС.900.Видео'),
(11, 'ПС.900.Пульт'),
(12, 'ПС.900.Корпус'),
(13, 'ПС.1200.Фото'),
(14, 'ПС.1200.Видео'),
(15, 'ПС.1200.Пульт'),
(16, 'ПС.1200.Корпус'),
(17, 'ПС.1500.Фото'),
(18, 'ПС.1500.Видео'),
(19, 'ПС.1500.Пульт'),
(20, 'ПС.1500.Корпус');
";
            using var command = new SQLiteCommand(query, _connection);
            command.ExecuteNonQuery();
        }

        private void FillProductComponent()
        {
            var query = @"
INSERT INTO ProductComponent (ProductId, ComponentId, Amount)
VALUES
-- ПС.400.Фото
(1,	1,		1),
(1,	6,		1),
(1,	11,		1),
(1,	12,		2),
(1,	13,		1),
(1,	14,		1),
(1,	15,		1),
(1,	16,		2),
(1,	17,		1),
(1,	22,		1),
(1,	27,		1),
(1,	31,		1),
(1,	32,		1),
(1,	34,		1),
(1,	35,		140),
(1,	36,		20),
(1,	39,		1),
(1,	40,		1),
(1,	41,		1),
(1,	52,		1),
(1,	53,		1),
(1,	54,		1),
(1,	55,		1),
(1,	56,		1),
(1,	57,		1),
(1,	58,		1),
(1,	59,		1),
(1,	60,		1),
(1,	61,		2),
(1,	62,		2),
(1,	63,		2),
(1,	64,		1),
(1,	65,		1),
(1,	66,		4),
(1,	67,		2),
(1,	69,		4),
(1,	70,		4),
(1,	71,		2),
(1,	72,		2),
(1,	73,		1),
(1,	74,		3),
(1,	75,		2),
(1,	76,		4),
(1,	79,		2),
(1,	80,		1),
(1,	81,		1),
(1,	82,		1),
(1,	83,		2),
(1,	84,		4),
(1,	85,		2),
(1,	86,		2),
(1,	90,		1),
(1,	91,		2),
(1,	92,		6),
(1,	95,		4),
(1,	98,		19),
(1,	99,		4),
(1,	100,	3),
(1,	102,	6),
(1,	103,	14),
(1,	104,	8),
(1,	105,	14),
(1,	109,	13),
(1,	110,	2),
(1,	111,	1),
(1,	112,	2),
(1,	113,	10),
(1,	114,	6),
(1,	115,	1),
(1,	116,	2),
(1,	117,	1),
(1,	119,	4),
(1,	120,	20),
(1,	121,	6),
(1,	123,	14),
(1,	125,	20),
(1,	126,	1),
(1,	127,	4),
(1,	130,	6),
(1,	131,	10),
(1,	133,	21),
(1,	134,	1),
(1,	136,	1),
(1,	137,	4),
(1,	139,	1),
(1,	142,	5),
(1,	143,	6),
(1,	144,	1),
(1,	148,	1),
(1,	149,	1),
(1,	150,	1),
(1,	151,	1),
-- ПС.400.Видео
(2,	1,		1),
(2,	6,		1),
(2,	11,		1),
(2,	12,		2),
(2,	13,		1),
(2,	14,		1),
(2,	15,		1),
(2,	16,		2),
(2,	17,		1),
(2,	23,		1),
(2,	27,		1),
(2,	31,		1),
(2,	32,		1),
(2,	33,		1),
(2,	35,		140),
(2,	36,		60),
(2,	39,		1),
(2,	40,		1),
(2,	41,		1),
(2,	53,		1),
(2,	54,		1),
(2,	55,		1),
(2,	56,		1),
(2,	57,		1),
(2,	59,		1),
(2,	60,		1),
(2,	64,		1),
(2,	65,		1),
(2,	66,		4),
(2,	67,		2),
(2,	69,		4),
(2,	70,		4),
(2,	71,		2),
(2,	73,		1),
(2,	74,		3),
(2,	75,		2),
(2,	76,		4),
(2,	77,		2),
(2,	79,		2),
(2,	80,		1),
(2,	81,		1),
(2,	82,		1),
(2,	83,		2),
(2,	84,		4),
(2,	85,		2),
(2,	86,		2),
(2,	90,		1),
(2,	91,		2),
(2,	92,		6),
(2,	95,		4),
(2,	98,		19),
(2,	99,		4),
(2,	100,	3),
(2,	102,	6),
(2,	103,	14),
(2,	104,	8),
(2,	105,	14),
(2,	109,	13),
(2,	110,	2),
(2,	111,	1),
(2,	113,	10),
(2,	114,	6),
(2,	115,	1),
(2,	116,	2),
(2,	117,	1),
(2,	118,	1),
(2,	119,	4),
(2,	120,	20),
(2,	121,	6),
(2,	123,	14),
(2,	125,	20),
(2,	126,	1),
(2,	127,	4),
(2,	130,	6),
(2,	131,	10),
(2,	133,	21),
(2,	134,	1),
(2,	135,	2),
(2,	136,	1),
(2,	137,	4),
(2,	142,	5),
(2,	143,	6),
(2,	144,	1),
(2,	148,	1),
(2,	149,	1),
(2,	150,	1),
(2,	151,	1),
-- ПС.400.Пульт
(3,	1,		1),
(3,	6,		1),
(3,	11,		1),
(3,	12,		2),
(3,	13,		1),
(3,	14,		1),
(3,	15,		1),
(3,	16,		2),
(3,	17,		1),
(3,	24,		1),
(3,	27,		1),
(3,	31,		1),
(3,	32,		1),
(3,	33,		1),
(3,	35,		140),
(3,	36,		60),
(3,	39,		1),
(3,	40,		1),
(3,	41,		1),
(3,	42,		1),
(3,	43,		1),
(3,	44,		1),
(3,	45,		2),
(3,	46,		1),
(3,	47,		2),
(3,	48,		1),
(3,	49,		1),
(3,	50,		1),
(3,	51,		1),
(3,	68,		4),
(3,	73,		1),
(3,	74,		3),
(3,	75,		2),
(3,	76,		4),
(3,	77,		2),
(3,	79,		2),
(3,	80,		1),
(3,	81,		1),
(3,	82,		1),
(3,	83,		2),
(3,	84,		4),
(3,	85,		2),
(3,	86,		2),
(3,	90,		1),
(3,	91,		2),
(3,	92,		6),
(3,	95,		4),
(3,	98,		19),
(3,	99,		4),
(3,	100,	3),
(3,	102,	6),
(3,	103,	14),
(3,	105,	14),
(3,	109,	13),
(3,	110,	2),
(3,	111,	1),
(3,	113,	10),
(3,	114,	6),
(3,	115,	1),
(3,	116,	2),
(3,	117,	1),
(3,	118,	1),
(3,	119,	4),
(3,	120,	20),
(3,	121,	6),
(3,	123,	14),
(3,	125,	20),
(3,	126,	1),
(3,	127,	4),
(3,	130,	6),
(3,	131,	10),
(3,	133,	21),
(3,	134,	1),
(3,	135,	2),
(3,	136,	1),
(3,	137,	4),
(3,	142,	5),
(3,	143,	6),
(3,	144,	1),
(3,	148,	1),
(3,	149,	1),
(3,	150,	1),
(3,	151,	1),
-- ПС.400.Корпус
(4,	1,		1),
(4,	6,		1),
(4,	11,		1),
(4,	12,		2),
(4,	13,		1),
(4,	14,		1),
(4,	15,		1),
(4,	16,		2),
(4,	17,		1),
(4,	25,		1),
(4,	27,		1),
(4,	31,		1),
(4,	32,		1),
(4,	33,		1),
(4,	35,		140),
(4,	36,		60),
(4,	39,		1),
(4,	40,		1),
(4,	41,		1),
(4,	42,		1),
(4,	50,		1),
(4,	68,		4),
(4,	73,		1),
(4,	74,		3),
(4,	75,		2),
(4,	76,		4),
(4,	77,		2),
(4,	79,		2),
(4,	80,		1),
(4,	81,		1),
(4,	82,		1),
(4,	83,		2),
(4,	84,		4),
(4,	85,		2),
(4,	86,		2),
(4,	90,		1),
(4,	91,		2),
(4,	92,		6),
(4,	95,		4),
(4,	98,		19),
(4,	99,		4),
(4,	100,	3),
(4,	102,	6),
(4,	103,	14),
(4,	105,	14),
(4,	109,	13),
(4,	110,	2),
(4,	111,	1),
(4,	113,	10),
(4,	114,	6),
(4,	115,	1),
(4,	116,	2),
(4,	117,	1),
(4,	118,	1),
(4,	119,	4),
(4,	120,	20),
(4,	121,	6),
(4,	123,	14),
(4,	125,	20),
(4,	126,	1),
(4,	127,	4),
(4,	130,	6),
(4,	131,	10),
(4,	133,	21),
(4,	134,	1),
(4,	135,	2),
(4,	136,	1),
(4,	137,	4),
(4,	142,	5),
(4,	143,	6),
(4,	144,	1),
(4,	148,	1),
(4,	149,	1),
(4,	150,	1),
(4,	151,	1),
-- ПС.600.Фото
(5,	2,		1),
(5,	7,		1),
(5,	11,		1),
(5,	12,		2),
(5,	13,		1),
(5,	14,		1),
(5,	15,		1),
(5,	16,		2),
(5,	17,		1),
(5,	22,		1),
(5,	27,		1),
(5,	31,		1),
(5,	32,		1),
(5,	34,		1),
(5,	35,		140),
(5,	36,		20),
(5,	39,		1),
(5,	40,		1),
(5,	41,		1),
(5,	52,		1),
(5,	53,		1),
(5,	54,		1),
(5,	55,		1),
(5,	56,		1),
(5,	57,		1),
(5,	58,		1),
(5,	59,		1),
(5,	60,		1),
(5,	61,		2),
(5,	62,		2),
(5,	63,		2),
(5,	64,		1),
(5,	65,		1),
(5,	66,		4),
(5,	67,		2),
(5,	69,		4),
(5,	70,		4),
(5,	71,		2),
(5,	72,		2),
(5,	73,		1),
(5,	74,		3),
(5,	75,		2),
(5,	76,		4),
(5,	79,		2),
(5,	80,		1),
(5,	81,		1),
(5,	82,		1),
(5,	83,		2),
(5,	84,		4),
(5,	85,		2),
(5,	86,		2),
(5,	90,		1),
(5,	91,		2),
(5,	93,		6),
(5,	95,		4),
(5,	98,		19),
(5,	99,		4),
(5,	100,	3),
(5,	102,	6),
(5,	103,	14),
(5,	104,	8),
(5,	105,	8),
(5,	108,	6),
(5,	109,	13),
(5,	110,	2),
(5,	111,	1),
(5,	112,	2),
(5,	113,	10),
(5,	114,	6),
(5,	115,	1),
(5,	116,	2),
(5,	117,	1),
(5,	119,	4),
(5,	120,	20),
(5,	121,	6),
(5,	123,	8),
(5,	124,	6),
(5,	125,	20),
(5,	126,	1),
(5,	127,	4),
(5,	130,	6),
(5,	131,	4),
(5,	132,	6),
(5,	133,	21),
(5,	134,	1),
(5,	136,	1),
(5,	137,	4),
(5,	140,	1),
(5,	142,	5),
(5,	143,	18),
(5,	144,	1),
(5,	148,	1),
(5,	149,	1),
(5,	150,	1),
(5,	151,	1),
-- ПС.600.Видео
(6,	2,		1),
(6,	7,		1),
(6,	11,		1),
(6,	12,		2),
(6,	13,		1),
(6,	14,		1),
(6,	15,		1),
(6,	16,		2),
(6,	17,		1),
(6,	23,		1),
(6,	27,		1),
(6,	31,		1),
(6,	32,		1),
(6,	33,		1),
(6,	35,		140),
(6,	36,		60),
(6,	39,		1),
(6,	40,		1),
(6,	41,		1),
(6,	53,		1),
(6,	54,		1),
(6,	55,		1),
(6,	56,		1),
(6,	57,		1),
(6,	59,		1),
(6,	60,		1),
(6,	64,		1),
(6,	65,		1),
(6,	66,		4),
(6,	67,		2),
(6,	69,		4),
(6,	70,		4),
(6,	71,		2),
(6,	73,		1),
(6,	74,		3),
(6,	75,		2),
(6,	76,		4),
(6,	77,		2),
(6,	79,		2),
(6,	80,		1),
(6,	81,		1),
(6,	82,		1),
(6,	83,		2),
(6,	84,		4),
(6,	85,		2),
(6,	86,		2),
(6,	90,		1),
(6,	91,		2),
(6,	93,		6),
(6,	95,		4),
(6,	98,		19),
(6,	99,		4),
(6,	100,	3),
(6,	102,	6),
(6,	103,	14),
(6,	104,	8),
(6,	105,	8),
(6,	108,	6),
(6,	109,	13),
(6,	110,	2),
(6,	111,	1),
(6,	113,	10),
(6,	114,	6),
(6,	115,	1),
(6,	116,	2),
(6,	117,	1),
(6,	118,	1),
(6,	119,	4),
(6,	120,	20),
(6,	121,	6),
(6,	123,	8),
(6,	124,	6),
(6,	125,	20),
(6,	126,	1),
(6,	127,	4),
(6,	130,	6),
(6,	131,	4),
(6,	132,	6),
(6,	133,	21),
(6,	134,	1),
(6,	135,	2),
(6,	136,	1),
(6,	137,	4),
(6,	142,	5),
(6,	143,	18),
(6,	144,	1),
(6,	148,	1),
(6,	149,	1),
(6,	150,	1),
(6,	151,	1);
-- ПС.600.Пульт
-- ПС.600.Корпус
-- ПС.900.Фото
-- ПС.900.Видео
-- ПС.900.Пульт
-- ПС.900.Корпус
-- ПС.1200.Фото
-- ПС.1200.Видео
-- ПС.1200.Пульт
-- ПС.1200.Корпус
-- ПС.1500.Фото
-- ПС.1500.Видео
-- ПС.1500.Пульт
-- ПС.1500.Корпус
";
            using var command = new SQLiteCommand(query, _connection);
            command.ExecuteNonQuery();
        }
    }
}
