using System.Data;

namespace Warehouse.Database
{
    public interface ISqlProvider
    {
        bool Connect();
        string[] GetComponentTypes();
        DataView GetComponents(int type);
    }
}
