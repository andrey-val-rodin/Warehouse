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
        public void IncrementComponentAmount_ValidAmount()
        {
            const int productId = 5;
            var components = SqlProvider.GetProductComponents(productId);
            var component = components.FirstOrDefault();

            SqlProvider.IncrementComponentAmount(component.Id);

            Assert.Equal(component.Amount + 1, SqlProvider.GetProductComponents(productId).FirstOrDefault().Amount);
        }

        [Fact]
        public void GetComponentId_ValidId()
        {
            var products = SqlProvider.GetProducts();
            var product = products.FirstOrDefault();

            var id = SqlProvider.GetComponentId(product.Name);

            var component = SqlProvider.GetComponents(0).FirstOrDefault(c => c.Id == id);
            Assert.Equal(product.Name, component.Name);
        }

        [Fact]
        public void GetProductId_ValidId()
        {
            var components = SqlProvider.GetComponents(0);
            var component = components.FirstOrDefault(c => c.IsUnit);

            var id = SqlProvider.GetProductId(component.Name);

            var product = SqlProvider.GetProducts().FirstOrDefault(p => p.Id == id);
            Assert.Equal(component.Name, product.Name);
        }

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
        public void GetMissingComponents_DoNotUseActualAmounts_ReturnsAllComponents()
        {
            // Prepare
            const int productId = 12;
            var components = PrepareMissingComponents(productId);

            // Action
            var newComponents = SqlProvider.GetMissingComponents(productId, false);

            // Assert
            Assert.Equal(components.Count(), newComponents.Count());
        }

        [Fact]
        public void GetMissingComponents_SufficientQuantityAndUseFreeAmounts_ReturnsEmptyCollection()
        {
            // Prepare
            const int productId = 14;
            PrepareMissingComponents(productId);

            // Action
            var newComponents = SqlProvider.GetMissingComponents(productId, true);

            // Assert
            Assert.Empty(newComponents);
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

        private IEnumerable<ProductComponent> PrepareMissingComponents(int productId)
        {
            var components = SqlProvider.GetProductComponents(productId);
            foreach (var c in components)
            {
                // Set Amount = Required in DB
                c.Amount = c.Required;
                c.AmountInUse = 0;
                SqlProvider.UpdateComponent(c);
            }

            // There should be sufficient amounts and there should be no missing components:
            Assert.Empty(SqlProvider.GetMissingComponents(productId, true));

            // Emulate creating new fabrication. This will set all AmountInUse = Required
            SqlProvider.AddProductAmountsInUse(productId);

            // Now all components are missing
            Assert.Equal(components.Count(), SqlProvider.GetMissingComponents(productId, false).Count());

            return components;
        }

        #endregion
    }
}