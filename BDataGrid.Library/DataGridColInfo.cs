using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public class DataGridColInfo<TItem>
        where TItem: class
    {
        public string? HeaderText { get; set; }

        public string? Width { get; set; }

        public Func<TItem, object?> ValueSelector { get; set; }

        public Func<TItem, string> Formatter { get; set; }
    }
}
