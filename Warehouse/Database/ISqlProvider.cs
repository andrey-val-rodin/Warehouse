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
        void AddProductAmounts(int productId);
        void SubtractProductAmounts(int productId);
    }
}
