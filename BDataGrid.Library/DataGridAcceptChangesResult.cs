using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public struct DataGridAcceptChangesResult
    {
        public DataGridAcceptChangesResult(string errorMessage): this(false, errorMessage)
        {
        }

        public DataGridAcceptChangesResult(bool changesApplied, string? errorMessage = null)
        {
            ChangesApplied = changesApplied;
            ErrorMessage = errorMessage;
        }

        public bool ChangesApplied { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
