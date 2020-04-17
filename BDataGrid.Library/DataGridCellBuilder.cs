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

            var cellInfo = rowInfo.Cells[PropertyName] = new DataGridCellInfo<TItem>();

            foreach (var action in Actions)
                action(rowInfo, cellInfo);
        }

        public DataGridCellBuilder<TItem, TProperty2> Property<TProperty2>(Expression<Func<TItem, TProperty2>> selector)
        {
            return DataGridRowBuilder.Property(selector);
        }

        public DataGridRowBuilder<TItem> If(Func<TItem, bool> condition)
        {
            return DataGridRowBuilder.If(condition);
        }

        public DataGridRowBuilder<TItem> ElseIf(Func<TItem, bool> condition)
        {
            return DataGridRowBuilder.ElseIf(condition);
        }

        public DataGridRowBuilder<TItem> Else()
        {
            return DataGridRowBuilder.Else();
        }

        public DataGridRowBuilder<TItem> Row()
        {
            return DataGridRowBuilder;
        }

        public DataGridCellBuilder<TItem, TProperty> HasColSpan(int colspan)
        {
            return AddAction((row, cell) => cell.ColSpan = colspan);
        }

        public DataGridCellBuilder<TItem, TProperty> HasClass(string classes)
        {
            return AddAction((row, cell) => cell.Classes = classes);
        }

        public DataGridCellBuilder<TItem, TProperty> HasBackgroundColor(System.Drawing.Color color)
        {
            return AddAction((row, cell) => cell.BackgroundColor = color);
        }
        public DataGridCellBuilder<TItem, TProperty> IsReadOnly()
        {
            return AddAction((row, cell) => cell.IsReadOnly = true);
        }
        public DataGridCellBuilder<TItem, TProperty> ReplaceWith(string content = "")
        {
            return AddAction((row, cell) => cell.FormatterString = item2 => content);
        }
        public DataGridCellBuilder<TItem, TProperty> Formatter(Func<TItem, string> formatter)
        {
            return AddAction((row, cell) => cell.FormatterString = formatter);
        }
    }
}
