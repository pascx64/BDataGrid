using System;
using System.Collections.Generic;

namespace BDataGrid.Library
{
    public class DataGridBuilder<TItem> : DataGridRowBuilder<TItem>
        where TItem : class
    {
        private Queue<Action<DataGridRowInfo<TItem>>> GlobalActions { get; } = new Queue<Action<DataGridRowInfo<TItem>>>();
        internal List<DataGridRowInfo<TItem>?> RowInfos { get; } = new List<DataGridRowInfo<TItem>?>();

        internal DataGridRowInfo<TItem> GlobalRowInfo { get; set; }

        internal Dictionary<string, DataGridColInfo<TItem>> Columns = new Dictionary<string, DataGridColInfo<TItem>>();

        public void Build(IReadOnlyList<TItem> items)
        {
            RowInfos.Clear();
            RowInfos.Capacity = items.Count;
            Columns.Clear();

            GlobalRowInfo = new DataGridRowInfo<TItem>();
            foreach (var action in GlobalActions)
                action(GlobalRowInfo);

            var cacheRowInfo = GlobalRowInfo.Clone();
            foreach (var item in items)
            {
                if (ExecuteActions(cacheRowInfo, item))
                {
                    RowInfos.Add(cacheRowInfo);
                    cacheRowInfo = GlobalRowInfo.Clone();
                }
                else
                    RowInfos.Add(null);
            }
        }

        public override void AddAction(Action<DataGridRowInfo<TItem>> action)
        {
            GlobalActions.Enqueue(action);
        }

        public new DataGridColBuilder<TItem, TProperty> Property<TProperty>(System.Linq.Expressions.Expression<Func<TItem, TProperty>> selector)
        {
            var cellBuilder = new DataGridColBuilder<TItem, TProperty>(selector, this);

            AddAction(cellBuilder.ExecuteActions);

            return cellBuilder;
        }
    }
}
