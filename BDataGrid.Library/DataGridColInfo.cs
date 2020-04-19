using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public class DataGridColInfo<TItem>
        where TItem : class
    {

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Id { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public string? HeaderText { get; set; }

        public string? Width { get; set; }

        public Func<TItem, object?> ValueSelector { get; set; }

        public Func<TItem, string> Formatter { get; set; }
    }
}
