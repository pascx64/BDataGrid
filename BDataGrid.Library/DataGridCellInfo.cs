using Microsoft.AspNetCore.Components;
using System;

namespace BDataGrid.Library
{
    public struct ValidationResult
    {
        public ValidationResult(bool isValid, string? errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public ValidationResult(string errorMessage) : this(false, errorMessage)
        {
        }

        public bool IsValid { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class DataGridCellInfo<TItem>
        where TItem : class
    {
        public System.Drawing.Color? BackgroundColor { get; set; }

        public string? Classes { get; set; }

        public int? ColSpan { get; set; }

        public bool? IsReadOnly { get; set; }

        public Func<TItem, string>? FormatterString { get; set; }

        public string? Append { get; set; }

        public Func<TItem, RenderFragment>? Formatter { get; set; }

        public DataGridEditorInfo<TItem>? EditorInfo { get; set; }

        public Func<TItem, object?, ValidationResult>? Validator { get; set; }

        public DataGridCellInfo<TItem> Clone()
        {
            return (DataGridCellInfo<TItem>)MemberwiseClone();
        }
    }
}
