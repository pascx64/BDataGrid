using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public partial class BDataGrid<TItem> : ComponentBase, IDisposable
        where TItem : class
    {
        [Parameter]
        public string TableCssClass { get; set; } = "";

        [Parameter]
        public IReadOnlyList<TItem> Items { get; set; }

        [Parameter]
        public Action<DataGridBuilder<TItem>> Configure { get; set; }

        [Parameter]
        public EventCallback<DataGridSelectedCellInfo<TItem>?> SelectedCellChanged { get; set; }

        Microsoft.JSInterop.DotNetObjectReference<BDataGrid<TItem>>? ThisReference { get; set; }

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
        public int PageSize { get; set; } = 500;

        public int CurrentTotalPages => (int)Math.Ceiling(Items.Count / (float)PageSize);

        public int CurrentPage { get; set; } = 0;

        internal DataGridBuilder<TItem> Builder { get; private set; }

        private ElementReference? TableRef { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (Builder == null)
            {
                Builder = new DataGridBuilder<TItem>();
                Configure(Builder);
                Builder.Build(Items);
            }
        }


        internal async Task SelectedCellFromClient(TItem? item, DataGridColInfo<TItem>? col, bool forceStateChange = false)
        {
            if (item != SelectedCell?.Item || col != SelectedCell?.Col)
            {
                SelectedCell = item == null || col == null ? null : new DataGridSelectedCellInfo<TItem>(item, col);
                CurrentEditor = null;
                CurrentEditorRenderFragment = null;

                await SelectedCellChanged.InvokeAsync(SelectedCell);

                if (forceStateChange)
                    StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
                await JSRuntime.InvokeAsync<bool>("BDataGrid.initializeDatagrid", new object?[] { ThisReference = DotNetObjectReference.Create(this), TableRef });

            if (SelectedCell != null && TableRef != null && SelectedCellChangedSinceLastRefresh)
                _ = TableRef.Value.FocusAsync(JSRuntime, ".selectedCell");
        }

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

        #region Editors

        internal DataGridEditorArgs? CurrentEditor { get; private set; }
        internal RenderFragment<DataGridEditorArgs>? CurrentEditorRenderFragment { get; private set; }

        public async Task OpenEditor(TItem item, DataGridColInfo<TItem> col, string? firstCharacter = null)
        {
            if (item != SelectedCell?.Item || col != SelectedCell?.Col)
            {
                if (CurrentEditor != null && CurrentEditor.ForceAccept != null)
                    await CurrentEditor.ForceAccept();

                await SelectedCellFromClient(item, col);
            }

            if (Builder.RowInfos[Items.IndexOf(item)]?.IsReadOnly ?? Builder.GlobalRowInfo.IsReadOnly ?? false)
                return;

            var cellInfo = GetCellInfo(item, col);
            if (cellInfo?.EditorInfo?.RenderFragmentProvider == null)
                return;

            CurrentEditor = new DataGridEditorArgs()
            {
                FirstCharacter = firstCharacter,
                Value = firstCharacter == null ? Builder.Columns[col.Id].ValueSelector(item) : null,
            };
            CurrentEditorRenderFragment = cellInfo.EditorInfo.RenderFragmentProvider(item);

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
                var currentIndex = Items.IndexOf(SelectedCell.Item);
                if (currentIndex <= 0)
                    return;

                await SelectedCellFromClient(Items[currentIndex - 1], FindColUnderCell(currentIndex - 1, SelectedCell.Col.Id));
            }
            else if (keyCode == 40) // down
            {
                var currentIndex = Items.IndexOf(SelectedCell.Item);
                if (currentIndex == -1 || currentIndex + 1 == Items.Count)
                    return;

                await SelectedCellFromClient(Items[currentIndex + 1], FindColUnderCell(currentIndex + 1, SelectedCell.Col.Id));
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

        public void Dispose()
        {
            ThisReference?.Dispose();
        }
    }
}
