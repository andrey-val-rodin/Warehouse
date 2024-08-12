using System.Data;
using Warehouse.Model;

namespace Warehouse.Database
{
    public interface ISqlProvider
    {
        bool Connect(string path);
        string[] GetComponentTypes();
        DataView GetComponents(int typeId);
        void UpdateComponent(Component component);
        string[] GetProductNames();
        DataView GetProductComponents(int productId);
        decimal GetProductPrice(int productId);
    }
}
