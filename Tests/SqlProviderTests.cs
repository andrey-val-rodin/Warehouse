using FluentAssertions;
using Warehouse.Database;
using Warehouse.Model;

namespace Tests
{
    public class DatabaseFixture : IDisposable
    {
        public SqlProvider SqlProvider { get; private set; }

        public DatabaseFixture()
        {
            var excel = "../../../../Components.xlsx";
            var db = "Components2.db";
            new WarehouseFiller.Filler().Fill(excel, db);

            SqlProvider = new SqlProvider();
            Assert.True(SqlProvider.Connect(db));
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
        public void AddProductAmountsInUse_ById7_CorrespondedAmountsInUseWereCorrectlyIncreased()
        {
            var productComponents = SqlProvider.GetProductComponents(7).ToArray();
            var oldAmountsInUse = GetAmountInUseValues(productComponents).ToArray();
            var requiredValues = GetRequiredValues(productComponents).ToArray();

            SqlProvider.AddProductAmountsInUse(7);

            var newAmountsInUse = GetAmountInUseValues(SqlProvider.GetProductComponents(7)).ToArray();

            Assert.Equal(productComponents.Length, newAmountsInUse.Length);
            Assert.Equal(oldAmountsInUse.Length, newAmountsInUse.Length);
            for (int i = 0; i < oldAmountsInUse.Length; i++)
            {
                Assert.Equal(oldAmountsInUse[i] + requiredValues[i], newAmountsInUse[i]);
            }
        }

        [Fact]
        public void SubtractProductAmountsInUse_ById17_CorrespondedAmountsInUseWereCorrectlyIncreased()
        {
            var productComponents = SqlProvider.GetProductComponents(17).ToArray();
            var oldAmountsInUse = GetAmountInUseValues(productComponents).ToArray();
            var requiredValues = GetRequiredValues(productComponents).ToArray();

            SqlProvider.SubtractProductAmountsInUse(17);

            var newAmountsInUse = GetAmountInUseValues(SqlProvider.GetProductComponents(17)).ToArray();

            Assert.Equal(productComponents.Length, newAmountsInUse.Length);
            Assert.Equal(oldAmountsInUse.Length, newAmountsInUse.Length);
            for (int i = 0; i < oldAmountsInUse.Length; i++)
            {
                Assert.Equal(oldAmountsInUse[i] - requiredValues[i], newAmountsInUse[i]);
            }
        }

        [Fact]
        public void SubtractProductAmounts_ById4_CorrespondedAmountsWereCorrectlyIncreased()
        {
            var productComponents = SqlProvider.GetProductComponents(4).ToArray();
            var oldAmounts = GetAmountValues(productComponents).ToArray();
            var requiredValues = GetRequiredValues(productComponents).ToArray();

            SqlProvider.SubtractProductAmounts(4);

            var newAmounts = GetAmountValues(SqlProvider.GetProductComponents(4)).ToArray();

            Assert.Equal(productComponents.Length, newAmounts.Length);
            Assert.Equal(oldAmounts.Length, newAmounts.Length);
            for (int i = 0; i < oldAmounts.Length; i++)
            {
                Assert.Equal(oldAmounts[i] - requiredValues[i], newAmounts[i]);
            }
        }

        [Fact]
        public void UpdateAllUnitPrices_AllPricesAreNull_PricesAreNotNull()
        {
            // Prepare
            var units = SqlProvider.GetComponents(0).Where(c => c.IsUnit);
            foreach (var u in units)
            {
                // Set field Price = NULL in DB
                u.Price = null;
                SqlProvider.UpdateComponent(u);
            }

            // Action
            SqlProvider.UpdateAllUnitPrices();

            // Assert
            units = SqlProvider.GetComponents(0).Where(c => c.IsUnit);
            foreach (var u in units)
            {
                Assert.NotNull(u.Price);
            }
        }

        [Fact]
        public void GetMissingComponents_AllComponentAmountsAreEqualToZero_ReturnsAllComponents()
        {
            // Prepare
            const int productId = 1;
            var components = SqlProvider.GetProductComponents(productId);
            foreach (var c in components)
            {
                // Set Amount = 0 in DB
                c.Amount = 0;
                SqlProvider.UpdateComponent(c);
            }

            // Action
            var newComponents = SqlProvider.GetMissingComponents(productId);

            // Assert
            Assert.Equal(components.Count(), newComponents.Count());
        }

        #region Helpers
        private static IEnumerable<int> GetRequiredValues(IEnumerable<ProductComponent> productComponents)
        {
            foreach (ProductComponent component in productComponents)
            {
                yield return component.Required;
            }
        }

        private static IEnumerable<int> GetAmountValues(IEnumerable<Component> components)
        {
            foreach (Component component in components)
            {
                yield return component.Amount;
            }
        }

        private static IEnumerable<int> GetAmountInUseValues(IEnumerable<Component> components)
        {
            foreach (Component component in components)
            {
                yield return component.AmountInUse;
            }
        }
        #endregion
    }
}