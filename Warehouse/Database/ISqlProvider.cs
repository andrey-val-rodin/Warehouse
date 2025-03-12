using Warehouse.Model;

namespace Warehouse.Database
{
    public interface ISqlProvider
    {
        bool Connect(string path);
        string[] GetComponentTypes();
        IEnumerable<Component> GetComponents(int typeId);
        void UpdateComponent(Component component);
        void IncrementComponentAmount(int componentId);
        int GetComponentId(string componentName);
        IEnumerable<Product> GetProducts();
        string[] GetProductNames();
        int GetProductId(string productName);
        IEnumerable<ProductComponent> GetProductComponents(int productId);
        decimal GetProductPrice(int productId);
        void UpdateAllUnitPrices();
        IEnumerable<ProductComponent> GetMissingComponents(int productId, bool useActualAmountsOnly);
        bool HasNegativeRemainders(int productId);
        void AddProductAmountsInUse(int productId);
        void SubtractProductAmountsInUse(int productId);
        void SubtractProductAmounts(int productId);
        int GetMaxFabricationNumber();
        IEnumerable<Fabrication> GetOpenedFabrications();
        IEnumerable<Fabrication> GetHistoricalFabrications();
        void InsertFabrication(Fabrication fabrication);
        void UpdateFabrication(Fabrication fabrication);
        void DeleteFabrication(int fabricationId);
    }
}
