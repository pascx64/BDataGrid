using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public enum FilterResult
    {
        Keep, Remove
    }
    public class DataGridColInfo<TItem>
        where TItem : class
    {

        public string Id { get; set; }

        public string? HeaderText { get; set; }

        public string? Width { get; set; }

        public Action<TItem, object?>? ValueSet { get; set; }

        public Func<TItem, object?> ValueSelector { get; set; }

        public Func<TItem, string> Formatter { get; set; }


        public object? CurrentFilterValue { get; set; }

        public Func<DataGridColInfo<TItem>, RenderFragment<BDataGrid<TItem>>> FilterRenderFragment { get; set; }

        public Func<DataGridColInfo<TItem>, DataGridRowInfo<TItem>, TItem, FilterResult>? FilterMethod { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
