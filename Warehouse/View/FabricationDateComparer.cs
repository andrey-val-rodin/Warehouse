using System.Collections;
using Warehouse.Model;
using ListSortDirection = System.ComponentModel.ListSortDirection;

namespace Warehouse.View
{
    public class FabricationDateComparer(ListSortDirection sortDirection, string sortMemberPath) : IComparer
    {
        public ListSortDirection SortDirection { get; } = sortDirection;
        public string SortMemberPath { get; } = sortMemberPath;

        public int Compare(object x, object y)
        {
            DateTime? dateX;
            DateTime? dateY;
            switch (SortMemberPath)
            {
                case "ExpectedDate":
                    dateX = (x as Fabrication)?.ExpectedDate;
                    dateY = (y as Fabrication)?.ExpectedDate;
                    break;
                case "StartedDate":
                    dateX = (x as Fabrication)?.StartedDate;
                    dateY = (y as Fabrication)?.StartedDate;
                    break;
                case "ClosedDate":
                    dateX = (x as Fabrication)?.ClosedDate;
                    dateY = (y as Fabrication)?.ClosedDate;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported sortMemberPath");
            }

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
