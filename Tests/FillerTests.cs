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
            Assert.Equal("���������� ����� ������ 100", exception.Message);
        }

        [Fact]
        public void Fill_Few�olumns_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase2.xlsx", "Components.db"));
            Assert.Equal("���������� �������� ������ 10", exception.Message);
        }

        [Fact]
        public void Fill_NoColumnName_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase3.xlsx", "Components.db"));
            Assert.Equal("������� ������������ �� ������", exception.Message);
        }

        [Fact]
        public void Fill_NoColumnPrice_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase4.xlsx", "Components.db"));
            Assert.Equal("������� ���� �� ������", exception.Message);
        }

        [Fact]
        public void Fill_NoColumnAmount_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase5.xlsx", "Components.db"));
            Assert.Equal("������� ���������� �� ������", exception.Message);
        }

        [Fact]
        public void Fill_NoUnits_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase6.xlsx", "Components.db"));
            Assert.Equal("���� �� �������", exception.Message);
        }

        [Fact]
        public void Fill_NoTables_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase7.xlsx", "Components.db"));
            Assert.Equal("����� �� �������", exception.Message);
        }

        [Fact]
        public void Fill_NoFirstGroup_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase8.xlsx", "Components.db"));
            Assert.Equal("�� ������� �������� ������ ������", exception.Message);
        }

        [Fact]
        public void Fill_EmptyGroup_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase9.xlsx", "Components.db"));
            Assert.Equal("���������� ������ ������", exception.Message);
        }

        [Fact]
        public void Fill_NoUnitsGroup_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase10.xlsx", "Components.db"));
            Assert.Equal("������ ���� �� �������", exception.Message);
        }

        [Fact]
        public void Fill_DuplicateComponent_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase11.xlsx", "Components.db"));
            Assert.Equal("����������� �������� ��������������: ����������.��.400  (��� 18 ��)", exception.Message);
        }

        [Fact]
        public void Fill_DuplicateProduct_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase12.xlsx", "Components.db"));
            Assert.Equal("����������� �������� �������: ��.400.����", exception.Message);
        }

        [Fact]
        public void Fill_NoUnit_FillerException()
        {
            var exception = Assert.Throws<FillerException>(() => _filler.Fill("../../../TestCase13.xlsx", "Components.db"));
            Assert.Equal("�� ������� ��� ����: ���� ����� ����", exception.Message);
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