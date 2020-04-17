using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public class BDataGridStyleEmpty : IBDataGridStyle
    {
        public string TableClass { get; set; } = "";

        public string RowClass { get; set; } = "";

        public string CellClass { get; set; } = "";

        public string HeaderClass { get; set; } = "";

        public string HeaderRowClass { get; set; } = "";
    }
}
