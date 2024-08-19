using System.Collections;
using Warehouse.Model;
using ListSortDirection = System.ComponentModel.ListSortDirection;

namespace Warehouse.View
{
    public class ComponentPriceComparer(ListSortDirection sortDirection) : IComparer
    {
        public ListSortDirection SortDirection { get; } = sortDirection;

        public int Compare(object x, object y)
        {
            var priceX = (x as Component)?.Price;
            var priceY = (y as Component)?.Price;
            return Compare(priceX, priceY);
        }

        public int Compare(decimal? x, decimal? y)
        {
            if (x == null && y == null)
                return 0;

            if (y == null)
                return -1;
            if (x == null)
                return 1;

            decimal xv = x.Value;
            decimal yv = y.Value;

            if (xv == yv)
                return 0;

            return xv > yv ?
                SortDirection == ListSortDirection.Ascending ? 1 : -1 :
                SortDirection == ListSortDirection.Ascending ? -1 : 1;
        }
    }
}
