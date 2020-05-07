using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public abstract class DataGridCellBuilderGeneric<TItem>
        where TItem : class
    {

        public abstract void AddAction(Action<DataGridRowInfo<TItem>, DataGridCellInfo<TItem>> action);

        public abstract void ExecuteActions(DataGridRowInfo<TItem> rowInfo);

        internal abstract DataGridCellBuilder<TItem, TProperty> GetProperty<TProperty>(Expression<Func<TItem, TProperty>> selector);

        internal abstract DataGridCellBuilderGeneric<TItem> HasAutoEditorGeneric();

        public DataGridCellBuilderGeneric<TItem> HasAutoEditor()
        {
            return HasAutoEditorGeneric();
        }

        public DataGridCellBuilderGeneric<TItem> HasColSpan(int colspan)
        {
            AddAction((_, cell) => cell.ColSpan = colspan);
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> HasClass(string classes, bool overrideExisting = true)
        {
            AddAction((_, cell) => cell.Classes = overrideExisting ? classes : ((cell.Classes ?? "") + " " + classes).Trim());
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> HasBackgroundColor(System.Drawing.Color color)
        {
            AddAction((_, cell) => cell.BackgroundColor = color);
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> HasBackgroundColor(string htmlColor)
        {
            AddAction((_, cell) => cell.BackgroundColor = System.Drawing.ColorTranslator.FromHtml(htmlColor));
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> IsReadOnly()
        {
            AddAction((_, cell) => cell.IsReadOnly = true);
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> IsNotReadOnly()
        {
            AddAction((_, cell) => cell.IsReadOnly = false);
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> IsEditable()
        {
            AddAction((_, cell) => cell.IsReadOnly = false);
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> ReplaceWith(string content = "")
        {
            AddAction((_, cell) => cell.FormatterString = _ => content);
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> HasFormatter(Func<TItem, string> formatter)
        {
            AddAction((_, cell) => cell.FormatterString = formatter);
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> HasAppendedText(string append)
        {
            AddAction((row, cell) => cell.Append = append);
            return this;
        }

        public DataGridCellBuilderGeneric<TItem> HasCellValueChangedCallback(Func<DataGridSelectedCellInfo<TItem>, Task>? onCellValueChanged)
        {
            AddAction((_, cell) => cell.OnCellValueChanged = onCellValueChanged);

            return this;
        }

        public DataGridCellBuilderGeneric<TItem> HasEditorValueConversion(Func<object?, EditorValueConversionResult>? editorValueConversion)
        {
            AddAction((_, cell) => cell.EditorValueConversion = editorValueConversion);

            return this;
        }

        
    }
}
