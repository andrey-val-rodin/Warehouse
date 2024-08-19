using System.Collections;
using Warehouse.Model;
using ListSortDirection = System.ComponentModel.ListSortDirection;

namespace Warehouse.View
{
    public class ComponentDateComparer(ListSortDirection sortDirection) : IComparer
    {
        public ListSortDirection SortDirection { get; } = sortDirection;

        public int Compare(object x, object y)
        {
            var dateX = (x as Component)?.ExpectedDate;
            var dateY = (y as Component)?.ExpectedDate;
            return Compare(dateX, dateY);
        }

        public int Compare(DateTime? x, DateTime? y)
        {
            if (x == null && y == null)
                return 0;

            if (y == null)
                return -1;
            if (x == null)
                return 1;

            DateTime xv = x.Value;
            DateTime yv = y.Value;

            if (xv == yv)
                return 0;

            return xv > yv ?
                SortDirection == ListSortDirection.Ascending ? 1 : -1 :
                SortDirection == ListSortDirection.Ascending ? -1 : 1;
        }
    }
}
