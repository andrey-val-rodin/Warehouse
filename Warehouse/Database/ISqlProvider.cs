using System.Data;
using Warehouse.Model;

namespace Warehouse.Database
{
    public interface ISqlProvider
    {
        bool Connect(string path);
        string[] GetComponentTypes();
        DataView GetComponents(int type);
        void UpdateComponent(Component component);
        string[] GetProductNames();
        DataView GetProductComponents(int type);
        decimal GetProductPrice(int type);
    }
}
