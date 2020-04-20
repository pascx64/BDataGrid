using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BDataGrid.Library
{
    public class DataGridCellBuilder<TItem, TProperty>
        where TItem : class
    {
        protected Expression<Func<TItem, TProperty>> Selector { get; set; }
        protected string PropertyName { get; private set; }
        private DataGridRowBuilder<TItem> DataGridRowBuilder { get; set; }

        public DataGridCellBuilder(Expression<Func<TItem, TProperty>> selector, DataGridRowBuilder<TItem> dataGridRowBuilder)
        {
            Selector = selector;
            PropertyName = selector.ToString();
            DataGridRowBuilder = dataGridRowBuilder;
        }
        private readonly Queue<Action<DataGridRowInfo<TItem>, DataGridCellInfo<TItem>>> Actions = new Queue<Action<DataGridRowInfo<TItem>, DataGridCellInfo<TItem>>>();

        public DataGridCellBuilder<TItem, TProperty> AddAction(Action<DataGridRowInfo<TItem>, DataGridCellInfo<TItem>> action)
        {
            Actions.Enqueue(action);
            return this;
        }
        public virtual void ExecuteActions(DataGridRowInfo<TItem> rowInfo)
        {
            if (rowInfo.Cells == null)
                rowInfo.Cells = new Dictionary<string, DataGridCellInfo<TItem>>();

            if (!rowInfo.Cells.TryGetValue(PropertyName, out var cellInfo))
                cellInfo = rowInfo.Cells[PropertyName] = new DataGridCellInfo<TItem>();

            foreach (var action in Actions)
                action(rowInfo, cellInfo);
        }

        public DataGridCellBuilder<TItem, TProperty2> Property<TProperty2>(Expression<Func<TItem, TProperty2>> selector)
        {
            return DataGridRowBuilder.Property(selector);
        }

        public DataGridCellBuilder<TItem, TProperty> If(Func<TItem, bool> condition)
        {
            var conditionalRowBuilder = DataGridRowBuilder.If(condition);

            return conditionalRowBuilder.Property(Selector);
        }
        public DataGridRowBuilder<TItem> IfRow(Func<TItem, bool> condition)
        {
           return DataGridRowBuilder.If(condition);
        }

        public DataGridRowBuilder<TItem> ElseIfRow(Func<TItem, bool> condition)
        {
            return DataGridRowBuilder.ElseIf(condition);

        }

        public DataGridCellBuilder<TItem, TProperty> ElseIf(Func<TItem, bool> condition)
        {
            var conditionalRowBuilder = DataGridRowBuilder.ElseIf(condition);

            return conditionalRowBuilder.Property(Selector);
        }

        public DataGridCellBuilder<TItem, TProperty> Else()
        {
            var conditionalRowBuilder = DataGridRowBuilder.Else();

            return conditionalRowBuilder.Property(Selector);
        }
        public DataGridRowBuilder<TItem> ElseRow()
        {
            return DataGridRowBuilder.Else();
        }

        public DataGridRowBuilder<TItem> EndIf()
        {
            return DataGridRowBuilder.EndIf();
        }

        public DataGridRowBuilder<TItem> Row()
        {
            return DataGridRowBuilder;
        }

        public DataGridCellBuilder<TItem, TProperty> HasColSpan(int colspan)
        {
            return AddAction((row, cell) => cell.ColSpan = colspan);
        }

        public DataGridCellBuilder<TItem, TProperty> HasClass(string classes, bool overrideExisting = true)
        {
            return AddAction((row, cell) => cell.Classes = overrideExisting ? classes : ((cell.Classes ?? "") + " " + classes).Trim());
        }

        public DataGridCellBuilder<TItem, TProperty> HasBackgroundColor(System.Drawing.Color color)
        {
            return AddAction((row, cell) => cell.BackgroundColor = color);
        }

        public DataGridCellBuilder<TItem, TProperty> HasBackgroundColor(string htmlColor)
        {
            return AddAction((row, cell) => cell.BackgroundColor = System.Drawing.ColorTranslator.FromHtml(htmlColor));
        }

        public DataGridCellBuilder<TItem, TProperty> IsReadOnly()
        {
            return AddAction((row, cell) => cell.IsReadOnly = true);
        }

        public DataGridCellBuilder<TItem, TProperty> ReplaceWith(string content = "")
        {
            return AddAction((row, cell) => cell.FormatterString = _ => content);
        }

        public DataGridCellBuilder<TItem, TProperty> HasFormatter(Func<TItem, string> formatter)
        {
            return AddAction((row, cell) => cell.FormatterString = formatter);
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor(Func<TItem, RenderFragment<DataGridEditorArgs>> renderFragmentProvider)
        {
            return AddAction((row, cell) =>
            {
                if (cell.EditorInfo == null)
                    cell.EditorInfo = new DataGridEditorInfo<TItem>();
                cell.EditorInfo.RenderFragmentProvider = renderFragmentProvider;
            });
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor(Type editorType, Func<TItem, object>? argsProvider = null)
        {
            return HasEditor(item =>
            {
                return args =>
                {
                    return builder =>
                    {
                        builder.OpenComponent(0, editorType);
                        builder.AddAttribute(1, "Args", args);
                        if (argsProvider != null)
                        {
                            var editorArgs = argsProvider(item);
                            int sequence = 1;
                            foreach (var property in editorArgs.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                                builder.AddAttribute(++sequence, property.Name, property.GetValue(editorArgs));
                        }
                        builder.CloseComponent();
                    };
                };
            });
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor<TEditor>()
            where TEditor : Editors.BDataGridEditorBase
        {
            return HasEditor(typeof(TEditor), null);
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor<TEditor>(object args)
            where TEditor : Editors.BDataGridEditorBase
        {
            return HasEditor(typeof(TEditor), _ => args);
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor<TEditor>(Func<TItem, object> argsProvider)
            where TEditor : Editors.BDataGridEditorBase
        {
            return HasEditor(typeof(TEditor), argsProvider);
        }

        public DataGridCellBuilder<TItem, TProperty> HasTextEditor()
        {
            return HasEditor<Editors.BDataGridEditor_Text>();
        }
    }
}
