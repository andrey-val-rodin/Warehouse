using ExcelDataReader.Exceptions;
using WarehouseFiller;

namespace Tests
{
    public class FillerTests()
    {
        private Filler _filler = new();

        [Fact]
        public void Fill_NullArgument_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => _filler.Fill(null, null));
        }

        [Fact]
        public void Fill_ExcelDoesNotExist_Exception()
        {
            Assert.Throws<FileNotFoundException>(() => _filler.Fill("wrong-name", "Components.db"));
        }

        [Fact]
        public void Fill_WrongExcelFile_ExcelReaderException()
        {
            Assert.Throws<HeaderException>(() => _filler.Fill("WarehouseFiller.runtimeconfig.json", "Components.db"));
        }

        [Fact]
        public void Fill_FewRows_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase1.xlsx", "Components.db"));
            Assert.Equal("Количество строк меньше 100", exception.Message);
        }

        [Fact]
        public void Fill_FewСolumns_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase2.xlsx", "Components.db"));
            Assert.Equal("Количество столбцов меньше 10", exception.Message);
        }

        [Fact]
        public void Fill_NoColumnName_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase3.xlsx", "Components.db"));
            Assert.Equal("Столбец Наименование не найден", exception.Message);
        }

        [Fact]
        public void Fill_NoColumnPrice_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase4.xlsx", "Components.db"));
            Assert.Equal("Столбец Цена не найден", exception.Message);
        }

        [Fact]
        public void Fill_NoColumnAmount_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase5.xlsx", "Components.db"));
            Assert.Equal("Столбец Количество не найден", exception.Message);
        }

        [Fact]
        public void Fill_NoUnits_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase6.xlsx", "Components.db"));
            Assert.Equal("Узлы не найдены", exception.Message);
        }

        [Fact]
        public void Fill_NoTables_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase7.xlsx", "Components.db"));
            Assert.Equal("Столы не найдены", exception.Message);
        }

        [Fact]
        public void Fill_NoFirstGroup_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase8.xlsx", "Components.db"));
            Assert.Equal("Не найдено название первой группы", exception.Message);
        }

        [Fact]
        public void Fill_EmptyGroup_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase9.xlsx", "Components.db"));
            Assert.Equal("Обнаружена пустая группа", exception.Message);
        }

        [Fact]
        public void Fill_NoUnitsGroup_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase10.xlsx", "Components.db"));
            Assert.Equal("Группа Узлы не найдена", exception.Message);
        }

        [Fact]
        public void Fill_DuplicateComponent_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase11.xlsx", "Components.db"));
            Assert.Equal("Дублирующее название комплектующего: Столешница.ПС.400  (МДФ 18 мм)", exception.Message);
        }

        [Fact]
        public void Fill_DuplicateProduct_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase12.xlsx", "Components.db"));
            Assert.Equal("Дублирующее название изделия: ПС.400.Фото", exception.Message);
        }

        [Fact]
        public void Fill_NoUnit_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase13.xlsx", "Components.db"));
            Assert.Equal("Не найдено имя узла: Узел платы фото", exception.Message);
        }

        [Fact]
        public void Fill()
        {
            var filler = new Filler();
            var excel = "../../../../Components.xlsx";
            var db = "Components.db";
            filler.Fill(excel, db);
            filler.Dispose();

            var inspector = new DbInspector();
            inspector.AssertDbValid(excel, db);
        }
    }
}