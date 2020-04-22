using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public class DataGridColumnInfoAttribute : Attribute
    {
        public string? Name { get; set; }

        public string? Width { get; set; }
    }
}
