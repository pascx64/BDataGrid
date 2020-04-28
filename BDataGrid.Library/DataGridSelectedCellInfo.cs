using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public class DataGridSelectedCellInfo<TItem>
        where TItem : class
    {
        public DataGridSelectedCellInfo(TItem item, DataGridColInfo<TItem> col, DataGridRowInfo<TItem> rowInfo)
        {
            Item = item;
            Col = col;
            RowInfo = rowInfo;
        }

        public TItem Item { get; }

        public DataGridColInfo<TItem> Col { get; }

        public DataGridRowInfo<TItem> RowInfo { get; internal set; }

        public DataGridCellInfo<TItem>? CellInfo => RowInfo.Cells == null ? null : RowInfo.Cells.TryGetValue(Col.Id, out var cellInfo) ? cellInfo : null;
    }
}
