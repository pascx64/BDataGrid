using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public class DataGridColBuilder<TItem, TProperty> : DataGridCellBuilder<TItem, TProperty>
        where TItem : class
    {

        private readonly DataGridBuilder<TItem> GridBuilder;
        public DataGridColBuilder(Expression<Func<TItem, TProperty>> selector, DataGridRowBuilder<TItem> dataGridRowBuilder) : base(selector, dataGridRowBuilder)
        {
            if (dataGridRowBuilder is DataGridBuilder<TItem> gridBuilder)
                GridBuilder = gridBuilder;
            else
                throw new Exception("Cannot create col builder any builder except the main grid builder");
        }

        private readonly Queue<Action<DataGridColInfo<TItem>>> ColActions = new Queue<Action<DataGridColInfo<TItem>>>();

        public DataGridColBuilder<TItem, TProperty> AddAction(Action<DataGridColInfo<TItem>> action)
        {
            ColActions.Enqueue(action);
            return this;
        }

        public new DataGridColBuilder<TItem, TProperty2> Property<TProperty2>(Expression<Func<TItem, TProperty2>> selector)
        {
            return GridBuilder.Property(selector);
        }

        public override void ExecuteActions(DataGridRowInfo<TItem> rowInfo)
        {
            DataGridColInfo<TItem> colInfo;
            if (!GridBuilder.Columns.TryGetValue(PropertyName, out var colInfo_) || colInfo_ == null)
            {
                var selector = Selector.Compile();
                colInfo = GridBuilder.Columns[PropertyName] = new DataGridColInfo<TItem>()
                {
                    Id = PropertyName,
                    ValueSelector = x => selector(x),
                    Formatter = x => selector(x)?.ToString() ?? ""
                };
            }
            else
                colInfo = colInfo_;

            foreach (var colAction in ColActions)
                colAction(colInfo);

            base.ExecuteActions(rowInfo);
        }

        public DataGridColBuilder<TItem, TProperty> HasWidth(string width)
        {
            return AddAction(c => c.Width = width);
        }

        public DataGridColBuilder<TItem, TProperty> HasHeaderText(string text)
        {
            return AddAction(c => c.HeaderText = text);
        }

        public new DataGridColBuilder<TItem, TProperty> HasAppendedText(string text)
        {
            return AddAction(c => c.Append = text);
        }
    }
}
