using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public class DataGridEditorInfo<TItem>
        where TItem : class
    {
        public Func<TItem, RenderFragment<DataGridEditorArgs>>? RenderFragmentProvider { get; set; }
    }
}
