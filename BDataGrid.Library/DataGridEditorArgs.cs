using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public class DataGridEditorArgs
    {
        public object? Value { get; set; }

        public string? FirstCharacter { get; set; }

        public Func<Task>? ForceAccept { get; set; }
    }
}
