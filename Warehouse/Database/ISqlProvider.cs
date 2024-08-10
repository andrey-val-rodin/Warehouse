using System.Data;

namespace Warehouse.Database
{
    public interface ISqlProvider
    {
        bool Connect(string path);
        string[] GetComponentTypes();
        DataView GetComponents(int type);
        void UpdateComponentAmount(int componentId, int amount);
        void UpdateComponentAmountInUse(int componentId, int amountInUse);
        void UpdateComponentPrice(int componentId, decimal? price);
    }
}
