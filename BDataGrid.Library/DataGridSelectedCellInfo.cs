using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public class DataGridSelectedCellInfo<TItem>
        where TItem : class
    {
        public DataGridSelectedCellInfo(TItem item, DataGridColInfo<TItem> col)
        {
            Item = item;
            Col = col;
        }

        public TItem Item { get; }

        public DataGridColInfo<TItem> Col { get; }
    }
}
