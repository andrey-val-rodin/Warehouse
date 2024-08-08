using System.Data;
using System.Reflection;

namespace Warehouse.Model
{
    class Converter
    {
        public static void Fill<T>(T model, DataRow row)
        {
            Dictionary<string, PropertyInfo> props = [];
            foreach (PropertyInfo p in model.GetType().GetProperties())
                props.Add(p.Name, p);

            foreach (DataColumn col in row.Table.Columns)
            {
                string name = col.ColumnName;
                if (row[name] != DBNull.Value && props.TryGetValue(name, out PropertyInfo value))
                {
                    object item = row[name];
                    PropertyInfo p = value;
                    if (p.PropertyType != col.DataType)
                        item = ChangeType(item, p.PropertyType);
                    p.SetValue(model, item, null);
                }
            }
        }

        public static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }
    }
}
