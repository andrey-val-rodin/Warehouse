using FluentAssertions;
using System.ComponentModel;
using System.Data;
using Warehouse.Database;

namespace Tests
{
    public class DatabaseFixture : IDisposable
    {
        public SqlProvider SqlProvider { get; private set; }

        public DatabaseFixture()
        {
            var builder = new DatabaseCreator();
            builder.CreateDatabase("Components.db");
            SqlProvider = new SqlProvider();
            Assert.True(SqlProvider.Connect("Components.db"));
        }

        public void Dispose()
        {
            SqlProvider.Dispose();
        }
    }

    public class SqlProviderTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture = fixture;
        private SqlProvider SqlProvider => _fixture.SqlProvider;

        [Fact]
        public void GetComponentTypes_CorrectTypes()
        {
            var result = SqlProvider.GetComponentTypes();

            result.Should().Equal([
                "Столешницы",
                "Детали стальные гнутые",
                "Токарка",
                "Заготовки",
                "Покупные изделия электрика",
                "Покупные изделия механика",
                "Метизы",
                "Болты",
                "Шайбы",
                "Гайки",
                "Саморезы",
                "Другое"
                ]);
        }

        [Fact]
        public void GetProductNames_CorrectNames()
        {
            var result = SqlProvider.GetProductNames();

            result.Should().Equal([
                "ПС.400.Фото",
                "ПС.400.Видео",
                "ПС.400.Пульт",
                "ПС.400.Корпус",
                "ПС.600.Фото",
                "ПС.600.Видео",
                "ПС.600.Пульт",
                "ПС.600.Корпус",
                "ПС.900.Фото",
                "ПС.900.Видео",
                "ПС.900.Пульт",
                "ПС.900.Корпус",
                "ПС.1200.Фото",
                "ПС.1200.Видео",
                "ПС.1200.Пульт",
                "ПС.1200.Корпус",
                "ПС.1500.Фото",
                "ПС.1500.Видео",
                "ПС.1500.Пульт",
                "ПС.1500.Корпус"
                ]);
        }

        [Fact]
        public void GetComponents_ByType10_CorrectIds()
        {
            var result = GetColumn(0, SqlProvider.GetComponents(10));

            result.Should().Equal([130, 131, 132, 133, 134, 135, 136]);
        }

        [Fact]
        public void GetComponents_ByType10_CorrectNames()
        {
            var result = GetColumn(2, SqlProvider.GetComponents(10));

            result.Should().Equal([
                "Гайка М3 DIN 934",
                "Гайка М4 DIN 934",
                "Гайка М5 DIN 934",
                "Гайка М6 DIN 934",
                "Гайка М10 DIN 934",
                "Гайка М12х1 DIN 439",
                "Гайка М16х1 DIN 439"
                ]);
        }

        [Fact]
        public void GetProductComponents_ById1_CorrectIds()
        {
            var result = GetColumn(0, SqlProvider.GetProductComponents(1));

            result.Should().Equal([
                1, 6, 11, 12, 13, 14, 15, 16, 17, 22, 27, 31, 32, 34, 35, 36, 39, 40, 41, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 69, 70, 71, 72, 73, 74, 75, 76, 79, 80, 81, 82, 83, 84, 85, 86, 90, 91, 92, 95, 98, 99, 100, 102, 103, 104, 105, 109, 110, 111, 112, 113, 114, 115, 116, 117, 119, 120, 121, 123, 125, 126, 127, 130, 131, 133, 134, 136, 137, 139, 142, 143, 144, 148, 149, 150, 151
                ]);
        }

        [Fact]
        public void GetProductComponents_ById1_CorrectRequired()
        {
            var result = GetColumn(2, SqlProvider.GetProductComponents(1));

            result.Should().Equal([
                1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 140, 20, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 4, 2, 4, 4, 2, 2, 1, 3, 2, 4, 2, 1, 1, 1, 2, 4, 2, 2, 1, 2, 6, 4, 19, 4, 3, 6, 14, 8, 14, 13, 2, 1, 2, 10, 6, 1, 2, 1, 4, 20, 6, 14, 20, 1, 4, 6, 10, 21, 1, 1, 4, 1, 5, 6, 1, 1, 1, 1, 1
                ]);
        }

        #region Helpers
        private static IEnumerable<object> GetColumn(int index, DataView dataView)
        {
            foreach (DataRow row in dataView.Table.Rows)
            {
                yield return row[index];
            }
        }
        #endregion
    }
}