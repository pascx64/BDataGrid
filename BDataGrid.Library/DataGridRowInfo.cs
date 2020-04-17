﻿using System.Collections.Generic;
using System.Linq;

namespace BDataGrid.Library
{
    public class DataGridRowInfo<TItem>
        where TItem: class
    {
        public System.Drawing.Color? BackgroundColor { get; set; }

        public string? Classes { get; set; }

        public bool? IsReadOnly { get; set; }

        public Dictionary<string, DataGridCellInfo<TItem>>? Cells { get; set; }

        public DataGridRowInfo<TItem> Clone()
        {
            var clone = (DataGridRowInfo<TItem>)MemberwiseClone();

            clone.Cells = Cells.ToDictionary(x => x.Key, x => x.Value.Clone());

            return clone;
        }
    }
}