using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public interface IBDataGridStyle
    {
        string TableClass { get; set; }

        string RowClass { get; set; }

        string CellClass { get; set; }

        string HeaderClass { get; set; }

        string HeaderRowClass { get; set; }

        string SelectedCell { get; set; }
    }
}
