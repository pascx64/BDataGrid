using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

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

    public struct EditorValueConversionResult
    {
        public EditorValueConversionResult(object? value)
        {
            ConversionWorked = true;
            ErrorMessage = null;
            Value = value;
        }

        public EditorValueConversionResult(string errorMessage)
        {
            ConversionWorked = false;
            ErrorMessage = errorMessage;
            Value = null;
        }
        public bool ConversionWorked { get; set; }
        
        public object? Value { get; set; }

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

        public RenderFragment<DataGridFormatterArgs>? Formatter { get; set; }

        public DataGridEditorInfo<TItem>? EditorInfo { get; set; }

        public Func<object?, EditorValueConversionResult>? EditorValueConversion { get; set; }

        public Func<TItem, object?, ValidationResult>? Validator { get; set; }

        public Func<DataGridSelectedCellInfo<TItem>, Task>? OnCellValueChanged { get; internal set; }

        public DataGridCellInfo<TItem> Clone()
        {
            return (DataGridCellInfo<TItem>)MemberwiseClone();
        }
    }
}
