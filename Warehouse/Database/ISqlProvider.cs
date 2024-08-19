using Warehouse.Model;

namespace Warehouse.Database
{
    public interface ISqlProvider
    {
        bool Connect(string path);
        string[] GetComponentTypes();
        IEnumerable<Component> GetComponents(int typeId);
        void UpdateComponent(Component component);
        string[] GetProductNames();
        IEnumerable<ProductComponent> GetProductComponents(int productId);
        decimal GetProductPrice(int productId);
        int GetMinProductRemainder(int productId);
        void AddProductAmountsInUse(int productId);
        void SubtractProductAmountsInUse(int productId);
        void SubtractProductAmounts(int productId);
        int GetMaxFabricationNumber();
        IEnumerable<Fabrication> GetOpenedFabrications();
        IEnumerable<Fabrication> GetHistoricalFabrications();
        void InsertFabrication(Fabrication fabrication);
        void UpdateFabrication(Fabrication fabrication);
    }
}
