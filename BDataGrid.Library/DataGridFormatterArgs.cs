using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public class DataGridFormatterArgs
    {
        public object? Value { get; set; }

        public object Item { get; set; }

        public EventCallback<object?> AcceptNewValue { get; set; }

        public bool IsReadOnly { get; set; }
    }
}
