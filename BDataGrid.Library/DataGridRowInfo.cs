using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public enum RowLocation
    {
        Header,
        Body,
        Footer
    }
    public class DataGridRowInfo<TItem>
        where TItem: class
    {
        public System.Drawing.Color? BackgroundColor { get; internal set; }

        public string? Classes { get; internal set; }

        public bool? IsReadOnly { get; internal set; }

        public RowLocation? RowLocation { get; internal set; }

        public bool? ForceRefresh { get; internal set; }

        public Func<DataGridSelectedCellInfo<TItem>, Task>? OnCellValueChanged { get; internal set; }

        public Dictionary<string, DataGridCellInfo<TItem>>? Cells { get; internal set; }

        public DataGridRowInfo<TItem> Clone()
        {
            var clone = (DataGridRowInfo<TItem>)MemberwiseClone();

            clone.Cells = Cells?.ToDictionary(x => x.Key, x => x.Value.Clone());

            return clone;
        }
    }
}
