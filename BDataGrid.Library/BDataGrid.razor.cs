using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public partial class BDataGrid<TItem> : ComponentBase, IDisposable
        where TItem : class
    {
        [Parameter]
        public string TableCssClass { get; set; } = "";

        private IReadOnlyList<TItem> Items_;
        [Parameter]
        public IReadOnlyList<TItem> Items
        {
            get => Items_;
            set
            {
                if (Items_ != value)
                {
                    Items_ = value;
                    ItemsChanged = true;
                }
            }
        }

        [Parameter]
        public Action<DataGridBuilder<TItem>> Configure { get; set; }

        [Parameter]
        public EventCallback<DataGridSelectedCellInfo<TItem>?> SelectedCellChanged { get; set; }

        DotNetObjectReference<BDataGrid<TItem>>? ThisReference { get; set; }

        private bool SelectedCellChangedSinceLastRefresh { get; set; }
        private DataGridSelectedCellInfo<TItem>? SelectedCell_;
        [Parameter]
        public DataGridSelectedCellInfo<TItem>? SelectedCell
        {
            get => SelectedCell_;
            set
            {
                if (SelectedCell_ != value)
                {
                    SelectedCell_ = value;
                    SelectedCellChangedSinceLastRefresh = true;
                }
            }
        }

        [Parameter]
        public int PageSize { get; set; } = 200;

        [Parameter]
        public string Width { get; set; } = "100%";

        [Parameter]
        public string ExportFileName { get; set; } = "export";

        [Parameter]
        public string Height { get; set; } = "100%";

        [Parameter]
        public string HeaderPadding { get; set; } = "2000px";

        public int CurrentTotalPages => (int)Math.Ceiling(Builder.FilteredItems.Count / (float)PageSize);

        private int CurrentPage_ = 0;
        public int CurrentPage
        {
            // Make sure we're not showing a page out of range of the current FilteredItems
            // also, Max(0,..) will prevent from showing the page '-1' if there aren't any pages to show
            get => Math.Max(0, Math.Min(CurrentTotalPages - 1, CurrentPage_));
            set => CurrentPage_ = value;
        }

        internal DataGridBuilder<TItem> Builder { get; private set; }

        private ElementReference? TableRef { get; set; }

        private bool ItemsChanged { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (Builder == null)
            {
                Builder = new DataGridBuilder<TItem>()
                {
                    BDataGrid = this
                };
                Configure(Builder);
                Builder.Build(Items);
            }
            else if (ItemsChanged)
            {
                ItemsChanged = false;
                CloseEditor();

                Builder.Build(Items);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await JSRuntime.InvokeAsync<bool>("BDataGrid.initializeDatagrid", new object?[] { ThisReference = DotNetObjectReference.Create(this), TableRef });

                if (BDataGridStyle.PopupInitializationJavascriptFunction != null && TableRef != null)
                    await JSRuntime.InvokeVoidAsync(BDataGridStyle.PopupInitializationJavascriptFunction, TableRef);
            }

            if (SelectedCell != null && TableRef != null && SelectedCellChangedSinceLastRefresh)
                _ = TableRef.Value.FocusAsync(JSRuntime, ".selectedCell");

        }

        #region Sorting + selected cell

        public void SortingDirectionChangedFromClient(DataGridColInfo<TItem> col)
        {
            var currentSortingCol = Builder.Columns.Values.FirstOrDefault(x => x.CurrentSortDirection != null);
            if (currentSortingCol != col)
            {
                if (currentSortingCol != null)
                    currentSortingCol.CurrentSortDirection = null;
                col.CurrentSortDirection = SortDirection.Ascending;
            }
            else
                col.CurrentSortDirection = col.CurrentSortDirection == SortDirection.Ascending ? SortDirection.Descending : (SortDirection?)null;

            Builder.FilterAndSort();
        }

        internal async Task SelectedCellFromClient(TItem? item, DataGridColInfo<TItem>? col, bool forceStateChange = false)
        {
            if (item != SelectedCell?.Item || col != SelectedCell?.Col)
            {
                if (CurrentEditor != null && CurrentEditor.ForceAccept != null)
                {
                    if (!await CurrentEditor.ForceAccept()) // changes not accepted
                        return;
                }

                if (item != null && col != null)
                {
                    var index = Builder.Items.IndexOf(item);
                    var rowInfo = Builder.RowInfos[index] ?? Builder.GlobalRowInfo;
                    SelectedCell = new DataGridSelectedCellInfo<TItem>(item, col, rowInfo);
                }
                else
                    SelectedCell = null;

                CloseEditor();

                await SelectedCellChanged.InvokeAsync(SelectedCell);

                if (forceStateChange)
                    StateHasChanged();
            }
        }

        #endregion

        private DataGridCellInfo<TItem>? GetCellInfo(int itemIndex, string col)
        {
            var info = Builder.RowInfos[itemIndex];

            if (info == null)
                return Builder.GlobalRowInfo.Cells == null ? null : Builder.GlobalRowInfo.Cells.TryGetValue(col, out var cellInfo) ? cellInfo : null;

            return info.Cells == null ? null : info.Cells.TryGetValue(col, out var cellInfo2) ? cellInfo2 : null;
        }

        private DataGridCellInfo<TItem>? GetCellInfo(TItem item, DataGridColInfo<TItem> col)
        {
            return GetCellInfo(Items.IndexOf(item), col.Id);
        }

        public void ClearColumns()
        {
            Builder.Rebuild(true);
        }

        public void Refresh()
        {
            Builder.Rebuild();

            if (SelectedCell != null)
            {
                var index = Builder.Items.IndexOf(SelectedCell.Item);
                SelectedCell.RowInfo = Builder.RowInfos[index] ?? Builder.GlobalRowInfo;
            }
        }

        #region Editors

        internal DataGridEditorArgs? CurrentEditor { get; private set; }
        internal RenderFragment<DataGridEditorArgs>? CurrentEditorRenderFragment { get; private set; }

        private async Task<bool> TryAcceptChanges(object? value)
        {
            if (SelectedCell == null)
                return false;

            var result = await Builder.TryAcceptChanges(SelectedCell.Item, value, SelectedCell.Col, SelectedCell.RowInfo, SelectedCell.CellInfo);

            if (!result.ChangesApplied)
                _ = JSRuntime.InvokeVoidAsync("BDataGrid.editorError", ".selectedCell", result.ErrorMessage ?? "Error while applying value");
            else
                CloseEditor();
            return result.ChangesApplied;
        }

        public async Task<bool> TryAcceptChangesFromFormatter(TItem item, object? newValue, DataGridColInfo<TItem> col, DataGridRowInfo<TItem> rowInfo, DataGridCellInfo<TItem>? cellInfo)
        {
            var result = await Builder.TryAcceptChanges(item, newValue, col, rowInfo, cellInfo);

            if (!result.ChangesApplied)
                _ = JSRuntime.InvokeVoidAsync("BDataGrid.editorError", ".selectedCell", result.ErrorMessage ?? "Error while applying value");

            return result.ChangesApplied;
        }

        public void CloseEditor()
        {
            CurrentEditor = null;
            CurrentEditorRenderFragment = null;

            _ = JSRuntime.InvokeVoidAsync("BDataGrid.switchEditionMode", TableRef, false);

            StateHasChanged();
        }

        public async Task OpenEditor(TItem item, DataGridColInfo<TItem> col, string? firstCharacter = null)
        {
            if (item != SelectedCell?.Item || col != SelectedCell?.Col)
            {
                await SelectedCellFromClient(item, col);

                if (CurrentEditor != null && CurrentEditor.ForceAccept != null)
                {
                    if (!await CurrentEditor.ForceAccept()) // changes not accepted
                        return;
                }
            }

            var itemIndex = Items.IndexOf(item);
            if (Builder.RowInfos[itemIndex]?.IsReadOnly ?? Builder.GlobalRowInfo.IsReadOnly ?? false)
                return;

            var cellInfo = GetCellInfo(itemIndex, col.Id);

            if (cellInfo?.IsReadOnly == true)
                return;

            if (cellInfo?.EditorInfo?.RenderFragmentProvider == null)
                return;

            CurrentEditor = new DataGridEditorArgs()
            {
                FirstCharacter = firstCharacter,
                Value = firstCharacter == null ? Builder.Columns[col.Id].ValueSelector(item) : null,
                TryAcceptChanges = TryAcceptChanges,
                CancelAndCloseEditor = new EventCallback(this, (Action)CloseEditor)
            };
            CurrentEditorRenderFragment = cellInfo.EditorInfo.RenderFragmentProvider(item);

            _ = JSRuntime.InvokeVoidAsync("BDataGrid.switchEditionMode", TableRef, true);
            StateHasChanged();
        }

        [JSInvokable]
        public async Task OnKeyDown(string key)
        {
            if (SelectedCell == null || CurrentEditor != null)
                return;

            await OpenEditor(SelectedCell.Item, SelectedCell.Col, key == "" ? null : key);
        }

        #endregion

        #region Arrow keys

        private DataGridColInfo<TItem> FindColUnderCell(int row, string col)
        {
            var colIndex = Builder.Columns.Keys.IndexOf(col);
            var rowInfo = Builder.RowInfos[row];

            // look for all the cols on the left to see if one is colspanned over the one we want to select
            int currentColIndex = 0;
            foreach (var leftCol in Builder.Columns.Values)
            {
                if (leftCol.Id == col) // found it !
                    return leftCol;

                int currentColSpan = 1;
                if (rowInfo?.Cells != null && rowInfo.Cells.TryGetValue(leftCol.Id, out var cellInfo) && cellInfo.ColSpan != null)
                    currentColSpan = cellInfo.ColSpan.Value;
                else if (Builder.GlobalRowInfo.Cells != null && Builder.GlobalRowInfo.Cells.TryGetValue(leftCol.Id, out var globalCellInfo) && globalCellInfo.ColSpan != null)
                    currentColSpan = globalCellInfo.ColSpan.Value;

                if (currentColIndex + currentColSpan > colIndex)
                    return leftCol;

                ++currentColIndex;
            }

            throw new Exception("Couldn't find col:" + col);
        }

        [JSInvokable]
        public async Task OnArrowKeysPressed(int keyCode)
        {
            if (SelectedCell == null)
                return;

            bool refresh = true;

            if (keyCode == 38) // up
            {
                var currentRow = Builder.FilteredItems.FirstOrDefault(x => x.Item == SelectedCell.Item);
                if (currentRow == default || currentRow.Index <= 0)
                    return;

                var filteredIndex = Builder.FilteredItems.IndexOf(currentRow);
                var nextRow = Builder.FilteredItems[filteredIndex - 1];
                await SelectedCellFromClient(nextRow.Item, FindColUnderCell(nextRow.Index, SelectedCell.Col.Id));

            }
            else if (keyCode == 40) // down
            {
                var currentRow = Builder.FilteredItems.FirstOrDefault(x => x.Item == SelectedCell.Item);
                if (currentRow == default || currentRow.Index == -1 || currentRow.Index + 1 == Items.Count)
                    return;

                var filteredIndex = Builder.FilteredItems.IndexOf(currentRow);
                var nextRow = Builder.FilteredItems[filteredIndex + 1];
                await SelectedCellFromClient(nextRow.Item, FindColUnderCell(nextRow.Index, SelectedCell.Col.Id));
            }
            else if (keyCode == 39) // right 
            {
                var colIndex = Builder.Columns.Values.IndexOf(SelectedCell.Col);
                var currentIndex = Items.IndexOf(SelectedCell.Item);
                var cells = Builder.RowInfos[currentIndex]?.Cells;
                int colSpan = 1;
                if (cells != null)
                    colSpan = cells.TryGetValue(SelectedCell.Col.Id, out var cellInfo) ? cellInfo.ColSpan ?? 1 : 1;
                var nextCol = colIndex + colSpan;

                if (nextCol >= Builder.Columns.Count)
                    return;

                await SelectedCellFromClient(SelectedCell.Item, Builder.Columns.Values.ElementAt(nextCol));
            }
            else if (keyCode == 37) // left 
            {
                var colIndex = Builder.Columns.Values.IndexOf(SelectedCell.Col);
                if (colIndex == 0) // can't go more left than that...
                    return;

                await SelectedCellFromClient(SelectedCell.Item, Builder.Columns.Values.ElementAt(colIndex - 1));
            }
            else
                refresh = false;


            if (refresh)
                StateHasChanged();
        }

        #endregion

        #region Col size changed

        [JSInvokable]
        public void OnColResizedFromClient(int colIndex, string width)
        {
            var col = Builder.Columns.Values.ElementAtOrDefault(colIndex);
            if (col == null)
                return;

            col.ForcedWidth = width;

            Builder.Rebuild();


            StateHasChanged();
        }
        #endregion

        #region Excel 

        private async Task ExportExcel()
        {
            byte[] arr;
            using (var stream = new MemoryStream())
            {
                Builder.ExportExcel(stream);

                stream.Position = 0;
                arr = stream.ToArray();
            }

            var str = Convert.ToBase64String(arr);

            await JSRuntime.InvokeVoidAsync("BDataGrid.saveAsFile", ExportFileName + ".xlsx", str);
        }

        #endregion

        public void Dispose()
        {
            ThisReference?.Dispose();
        }
    }
}
