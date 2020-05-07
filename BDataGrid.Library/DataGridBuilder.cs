using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public class DataGridBuilder<TItem> : DataGridRowBuilder<TItem>
        where TItem : class
    {
        private Queue<Action<DataGridRowInfo<TItem>>> GlobalActions { get; } = new Queue<Action<DataGridRowInfo<TItem>>>();

        internal List<DataGridRowInfo<TItem>?> RowInfos_ = new List<DataGridRowInfo<TItem>?>();
        public IReadOnlyList<DataGridRowInfo<TItem>?> RowInfos => RowInfos_;

        public DataGridRowInfo<TItem> GlobalRowInfo { get; internal set; }

        internal Dictionary<string, DataGridColInfo<TItem>> Columns_ = new Dictionary<string, DataGridColInfo<TItem>>();

        public IReadOnlyDictionary<string, DataGridColInfo<TItem>> Columns => Columns_;

        internal BDataGrid<TItem> BDataGrid { get; set; }

        public DataGridBuilder() : base(null, null)
        {
        }

        internal bool ShowHeaderFilters { get; set; }

        internal bool AllowHeaderSorting { get; set; } = true;

        public IReadOnlyList<TItem> Items { get; private set; }
        public IReadOnlyList<(int Index, TItem Item)> AllBodyItems { get; private set; } = new List<(int Index, TItem Item)>();
        public IReadOnlyList<(int Index, TItem Item)> FilteredItems { get; private set; } = new List<(int Index, TItem Item)>();
        public IReadOnlyList<(int Index, TItem Item)> HeaderItems { get; private set; } = new List<(int Index, TItem Item)>();
        public IReadOnlyList<(int Index, TItem Item)> FooterItems { get; private set; } = new List<(int Index, TItem Item)>();

        public void Build(IReadOnlyList<TItem> items)
        {
            Items = items ?? new List<TItem>();

            Rebuild(true);
        }

        public void Rebuild(bool clearColumns = false)
        {
            RowInfos_.Clear();
            RowInfos_.Capacity = Items.Count;
            if (clearColumns)
                Columns_.Clear();
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
                    RowInfos_.Add(cacheRowInfo);

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
                    RowInfos_.Add(null);

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

        public FilterResult DefaultFilterMethod(DataGridColInfo<TItem> colInfo, DataGridRowInfo<TItem> rowInfo, TItem item)
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


        public async Task<DataGridAcceptChangesResult> TryAcceptChanges(TItem item, object? value, DataGridColInfo<TItem> col, DataGridRowInfo<TItem> rowInfo, DataGridCellInfo<TItem>? cellInfo)
        {
            object? valueConverted;
            if (cellInfo?.EditorValueConversion != null)
            {
                var result = cellInfo.EditorValueConversion(value);
                if (!result.ConversionWorked)
                    return new DataGridAcceptChangesResult(result.ErrorMessage ?? "Can't convert value");
                else
                    valueConverted = result.Value;
            }
            else
                valueConverted = value;

            var validator = cellInfo?.Validator;
            if (validator != null)
            {
                var result = validator(item, valueConverted);

                if (!result.IsValid)
                    new DataGridAcceptChangesResult(result.ErrorMessage ?? "Validation failed");
            }
            if (col.ValueSet == null)
                throw new Exception("Unable to apply new value, no 'ValueSet' is available");


            col.ValueSet(item, valueConverted);

            var selectedCell = new DataGridSelectedCellInfo<TItem>(item, col, rowInfo);

            if (cellInfo?.OnCellValueChanged != null)
                await cellInfo.OnCellValueChanged(selectedCell);

            if (rowInfo.OnCellValueChanged != null)
                await rowInfo.OnCellValueChanged(selectedCell);

            return new DataGridAcceptChangesResult(true);
        }

        public override void AddAction(Action<DataGridRowInfo<TItem>> action)
        {
            GlobalActions.Enqueue(action);
        }

        public new DataGridColBuilder<TItem, TProperty> Property<TProperty>(Expression<Func<TItem, TProperty>> selector)
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

        public DataGridBuilder<TItem> HasClassColumnAttributes()
        {
            var properties = typeof(TItem).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fields = typeof(TItem).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in properties.OfType<MemberInfo>().Concat(fields))
            {
                var attribute = (DataGridColumnInfoAttribute?)member.GetCustomAttribute<DataGridColumnInfoAttribute>(true);

                if (attribute != null)
                {
                    var parameter = Expression.Parameter(typeof(TItem), "p");
                    var body = Expression.MakeMemberAccess(parameter, member);

                    var lambda = Expression.Lambda(body, parameter);

                    var memberType = (member as FieldInfo)?.FieldType ?? ((PropertyInfo)member).PropertyType;
                    var method = GetType().GetMethods().First(x => x.Name == nameof(Property));
                    var propertyMethod = method.MakeGenericMethod(memberType);

                    var colProperty = propertyMethod.Invoke(this, new object[] { lambda });


                    if (attribute.Width != null)
                    {
                        var hasWidth = colProperty.GetType().GetMethod(nameof(DataGridColBuilder<object, int>.HasWidth));
                        hasWidth.Invoke(colProperty, new object[] { attribute.Width });
                    }
                    if (attribute.Name != null)
                    {
                        var hasWidth = colProperty.GetType().GetMethod(nameof(DataGridColBuilder<object, int>.HasHeaderText));
                        hasWidth.Invoke(colProperty, new object[] { attribute.Name });
                    }

                    if (attribute.AllowEdit)
                    {
                        var hasEditor = colProperty.GetType().GetMethod(nameof(DataGridColBuilder<object, int>.HasAutoEditor));
                        hasEditor.Invoke(colProperty, new object[0]);
                    }
                }
            }
            return this;
        }

        public void ExportExcel(string filePath)
        {
            using var file = System.IO.File.OpenWrite(filePath);

            ExportExcel(file);
        }

        public void ExportExcel(System.IO.Stream outputStream)
        {
            //Creates a blank workbook. Use the using statment, so the package is disposed when we are done.
            using var p = new ExcelPackage();

            var ws = p.Workbook.Worksheets.Add("Sheet");

            ws.Cells[1, 2, 1, Columns.Count].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, 2, 1, Columns.Count].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5A5A5A"));

            int colIndex = 0;
            foreach (var col in Columns)
            {
                var header = col.Value.HeaderText;
                if (string.IsNullOrEmpty(header))
                    header = "Unknowned header";
                ws.Cells[1, ++colIndex].Value = header;
            }


            var oddColor = System.Drawing.ColorTranslator.FromHtml("#c0c0c0");

            int rowIndex = 1;
            foreach (var row in FilteredItems)
            {
                var rowInfo = RowInfos[row.Index];
                int colSpanLeft = 1;
                colIndex = 0;
                var rowStart = ++rowIndex;

                if (rowStart % 2 == 0)
                {
                    ws.Cells[rowStart, colIndex, rowStart, Columns.Count].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[rowStart, colIndex, rowStart, Columns.Count].Style.Fill.BackgroundColor.SetColor(oddColor);
                }

                foreach (var col in Columns)
                {
                    --colSpanLeft;
                    var colStart = ++colIndex;
                    if (colSpanLeft != 0)
                        continue;

                    DataGridCellInfo<TItem>? cellInfo = null;
                    rowInfo?.Cells?.TryGetValue(col.Key, out cellInfo);

                    colSpanLeft = cellInfo?.ColSpan ?? 1;
                    var excelCell = ws.Cells[rowStart, colStart, rowStart, colStart + colSpanLeft - 1];

                    var valueStr = (cellInfo?.FormatterString ?? col.Value.Formatter)(row.Item);

                    if (double.TryParse(valueStr, out var valueDouble))
                        excelCell.Value = valueDouble;
                    else
                        excelCell.Value = valueStr;

                    excelCell.Merge = true;

                    if (cellInfo?.BackgroundColor != null)
                    {
                        excelCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        excelCell.Style.Fill.BackgroundColor.SetColor(cellInfo.BackgroundColor.Value);
                    }
                }
            }

            ws.Cells[1, 2, rowIndex, Columns.Count].AutoFilter = true;
            ws.Cells[1, 2, rowIndex, Columns.Count].AutoFitColumns();

            p.SaveAs(outputStream);
        }
    }
}
