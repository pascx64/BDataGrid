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

        public DataGridBuilder() : base(null, null)
        {
        }

        public IReadOnlyList<TItem> Items { get; private set; }
        public IReadOnlyList<(int Index, TItem Item)> FilteredItems { get; private set; }
        public IReadOnlyList<(int Index, TItem Item)> HeaderItems { get; private set; }
        public IReadOnlyList<(int Index, TItem Item)> FooterItems { get; private set; }

        public void Build(IReadOnlyList<TItem> items)
        {
            Items = items;
            RowInfos.Clear();
            RowInfos.Capacity = items.Count;
            Columns.Clear();

            GlobalRowInfo = new DataGridRowInfo<TItem>();
            foreach (var action in GlobalActions)
                action(GlobalRowInfo);

            var filteredItems = new List<(int Index, TItem item)>(items.Count);
            var headerItems = new List<(int Index, TItem item)>(3);
            var footerItems = new List<(int Index, TItem item)>(3);

            var cacheRowInfo = GlobalRowInfo.Clone();
            for (int i = 0; i < items.Count; ++i)
            {
                var item = items[i];
                if (ExecuteActions(cacheRowInfo, item))
                {
                    RowInfos.Add(cacheRowInfo);

                    var location = cacheRowInfo.RowLocation ?? RowLocation.Body;
                    if (location == RowLocation.Body)
                        filteredItems.Add((i, item));
                    else if (location == RowLocation.Footer)
                        footerItems.Add((i, item));
                    else if (location == RowLocation.Header)
                        headerItems.Add((i, item));

                    cacheRowInfo = GlobalRowInfo.Clone();
                }
                else
                {
                    RowInfos.Add(null);

                    filteredItems.Add((i, item));
                }
            }

            FilteredItems = filteredItems;
            HeaderItems = headerItems;
            FooterItems = footerItems;
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
