using FluentAssertions;
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
            builder.CreateDatabase("Component.db");
            SqlProvider = new SqlProvider();
            Assert.True(SqlProvider.Connect("Component.db"));
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

        private IEnumerable<object> GetColumn(int index, DataView dataView)
        {
            foreach (DataRow row in dataView.Table.Rows)
            {
                yield return row[index];
            }
        }
    }
}