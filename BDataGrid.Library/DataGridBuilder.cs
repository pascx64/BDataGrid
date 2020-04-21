using System;
using System.Collections.Generic;
using System.Linq;

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

        internal bool ShowHeaderFilters { get; set; }

        internal bool AllowHeaderSorting { get; set; } = true;

        public IReadOnlyList<TItem> Items { get; private set; }
        public IReadOnlyList<(int Index, TItem Item)> AllBodyItems { get; private set; }
        public IReadOnlyList<(int Index, TItem Item)> FilteredItems { get; private set; }
        public IReadOnlyList<(int Index, TItem Item)> HeaderItems { get; private set; }
        public IReadOnlyList<(int Index, TItem Item)> FooterItems { get; private set; }

        public void Build(IReadOnlyList<TItem> items)
        {
            Items = items;

            Rebuild();
        }

        public void Rebuild()
        {
            RowInfos.Clear();
            RowInfos.Capacity = Items.Count;
            Columns.Clear();
            ShowHeaderFilters = false;

            GlobalRowInfo = new DataGridRowInfo<TItem>();
            foreach (var action in GlobalActions)
                action(GlobalRowInfo);

            var allBodyItems = new List<(int Index, TItem item)>(Items.Count);
            var headerItems = new List<(int Index, TItem item)>(3);
            var footerItems = new List<(int Index, TItem item)>(3);

            var cacheRowInfo = GlobalRowInfo.Clone();
            for (int i = 0; i < Items.Count; ++i)
            {
                var item = Items[i];
                if (ExecuteActions(cacheRowInfo, item))
                {
                    RowInfos.Add(cacheRowInfo);

                    var location = cacheRowInfo.RowLocation ?? RowLocation.Body;
                    if (location == RowLocation.Body)
                        allBodyItems.Add((i, item));
                    else if (location == RowLocation.Footer)
                        footerItems.Add((i, item));
                    else if (location == RowLocation.Header)
                        headerItems.Add((i, item));

                    cacheRowInfo = GlobalRowInfo.Clone();
                }
                else
                {
                    RowInfos.Add(null);

                    allBodyItems.Add((i, item));
                }
            }

            AllBodyItems = allBodyItems;
            HeaderItems = headerItems;
            FooterItems = footerItems;

            FilterAndSort();
        }


        public void FilterAndSort()
        {
            List<(string? str, (int Index, TItem item))> sortedItems = new List<(string? str, (int Index, TItem item))>(AllBodyItems.Count);
            DataGridColInfo<TItem>? sortingCol = null;

            if (AllowHeaderSorting)
                sortingCol = Columns.Values.FirstOrDefault(x => x.CurrentSortDirection != null);

            foreach (var row in AllBodyItems)
            {
                var rowInfo = RowInfos[row.Index] ?? GlobalRowInfo;
                if (!IsFilteredOut(rowInfo, row.Item))
                {
                    if (sortingCol?.CurrentSortDirection != null)
                    {
                        var formatterString = rowInfo.Cells == null ? null : rowInfo.Cells.TryGetValue(sortingCol.Id, out var cellInfo) ? cellInfo.FormatterString : null;

                        var str = (formatterString ?? sortingCol.Formatter)(row.Item);
                        sortedItems.Add((str, row));
                    }
                    else
                        sortedItems.Add((str: null, row));
                }
            }

            if (sortingCol?.CurrentSortDirection != null)
                FilteredItems = (sortingCol.CurrentSortDirection == SortDirection.Ascending ? sortedItems.OrderBy(x => x.str) : sortedItems.OrderByDescending(x => x.str)).Select(x => x.Item2).ToList();
            else
                FilteredItems = sortedItems.Select(x => x.Item2).ToList();
        }

        private bool IsFilteredOut(DataGridRowInfo<TItem> rowInfo, TItem item)
        {
            if (!ShowHeaderFilters)
                return false;

            foreach (var col in Columns)
            {
                if (col.Value.CurrentFilterValue == null || col.Value.CurrentFilterValue as string == "")
                    continue;

                var method = col.Value.FilterMethod ?? DefaultFilterMethod;

                if (method(col.Value, rowInfo, item) == FilterResult.Remove)
                    return true;
            }

            return false;
        }

        private FilterResult DefaultFilterMethod(DataGridColInfo<TItem> colInfo, DataGridRowInfo<TItem> rowInfo, TItem item)
        {
            var strFilter = colInfo.CurrentFilterValue?.ToString() ?? "";
            if (string.IsNullOrEmpty(strFilter))
                return FilterResult.Keep;

            var formatterString = rowInfo.Cells == null ? null : rowInfo.Cells.TryGetValue(colInfo.Id, out var cellInfo) ? cellInfo.FormatterString : null;

            var str = (formatterString ?? colInfo.Formatter)(item);

            if (string.IsNullOrEmpty(str))
                return FilterResult.Remove; // if the filter contains something and the string doesn't.. rage quit here

            return str.Contains(strFilter, StringComparison.OrdinalIgnoreCase) ? FilterResult.Keep : FilterResult.Remove;
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

        public DataGridBuilder<TItem> HasFilterRow()
        {
            AddAction(row => ShowHeaderFilters = true);

            return this;
        }

        public DataGridBuilder<TItem> HasNoFilterRow()
        {
            AddAction(row => ShowHeaderFilters = false);

            return this;
        }

        public DataGridBuilder<TItem> HasNoSorting()
        {
            AddAction(row => AllowHeaderSorting = false);

            return this;
        }
    }
}
