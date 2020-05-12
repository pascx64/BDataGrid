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
    public enum SortDirection
    {
        Ascending, Descending
    }
    public class DataGridColInfo<TItem>
        where TItem : class
    {
        public string Id { get; set; }

        public string? HeaderText { get; set; }

        private string? Width_;
        public string? Width
        {
            get => ForcedWidth ?? Width_;
            set => Width_ = value;
        }

        public string? ForcedWidth { get; set; }

        public Action<TItem, object?>? ValueSet { get; set; }

        public Func<TItem, object?> ValueSelector { get; set; }

        public Func<TItem, string> Formatter { get; set; }

        public Type PropertyType { get; set; }

        public object? CurrentFilterValue { get; set; }

        public SortDirection? CurrentSortDirection { get; set; }

        public bool AutoWidthExcel { get; set; }

        public bool IsFixed { get; set; } = false;

        public string? PopupTitle { get; set; } = null;

        public string? PopupContent { get; set; } = null;

        public RenderFragment<BDataGrid<TItem>> FilterRenderFragment { get; set; }

        public Func<DataGridColInfo<TItem>, DataGridRowInfo<TItem>, TItem, FilterResult>? FilterMethod { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
