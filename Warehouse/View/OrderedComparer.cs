using System.Collections;
using Warehouse.Model;
using ListSortDirection = System.ComponentModel.ListSortDirection;

namespace Warehouse.View
{
    public class OrderedComparer(ListSortDirection sortDirection) : IComparer
    {
        public ListSortDirection SortDirection { get; set; } = sortDirection;

        public int Compare(object x, object y)
        {
            var orderedX = (x as Component)?.Ordered;
            var orderedY = (y as Component)?.Ordered;
            return Compare(orderedX, orderedY);
        }

        public int Compare(int? x, int? y)
        {
            if (x == null && y == null)
                return 0;

            if (y == null)
                return -1;
            if (x == null)
                return 1;

            int xv = x.Value;
            int yv = y.Value;

            if (xv == yv)
                return 0;

            return xv > yv ?
                SortDirection == ListSortDirection.Ascending ? 1 : -1 :
                SortDirection == ListSortDirection.Ascending ? -1 : 1;
        }
    }
}
