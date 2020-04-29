using Microsoft.AspNetCore.Components;
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
                colInfo = GridBuilder.Columns_[PropertyName] = new DataGridColInfo<TItem>()
                {
                    Id = PropertyName,
                    ValueSelector = x => SelectorFunc(x),
                    HeaderText = PropertyName,
                    Formatter = x => SelectorFunc(x)?.ToString() ?? "",
                    PropertyType = typeof(TItem),
                    ValueSet = SetFunc,
                    AutoWidthExcel = true
                };
                colInfo.FilterRenderFragment = GetFilterFormatter(typeof(Filters.DataGridCellFilter_Textbox), null)(colInfo);
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

        public DataGridColBuilder<TItem, TProperty> HasFilterMethod(Func<DataGridColInfo<TItem>, DataGridRowInfo<TItem>, TItem, FilterResult> method)
        {
            return AddAction(col => col.FilterMethod = method);
        }

        public DataGridColBuilder<TItem, TProperty> HasFilterFormatter(Func<DataGridColInfo<TItem>, RenderFragment<BDataGrid<TItem>>> renderFragmentProvider)
        {
            return AddAction((col) => col.FilterRenderFragment = renderFragmentProvider(col));
        }

        private Func<DataGridColInfo<TItem>, RenderFragment<BDataGrid<TItem>>> GetFilterFormatter(Type editorType, object? args)
        {
            return col =>
            {
                return bdatagrid =>
                {
                    return builder =>
                    {
                        builder.OpenComponent(0, editorType);
                        builder.AddAttribute(1, "Filter", col.CurrentFilterValue);
                        builder.AddAttribute(2, "FilterChanged", new EventCallback<object?>(bdatagrid, (Action<object?>)(value =>
                        {
                            col.CurrentFilterValue = value;

                            GridBuilder.FilterAndSort();
                        })));
                        if (args != null)
                        {
                            int sequence = 2;
                            foreach (var property in args.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                                builder.AddAttribute(++sequence, property.Name, property.GetValue(args));
                        }
                        builder.CloseComponent();
                    };
                };
            };
        }

        public DataGridColBuilder<TItem, TProperty> HasFilterFormatter(Type editorType, object? editorArgs = null)
        {
            return HasFilterFormatter(GetFilterFormatter(editorType, editorArgs));
        }

        public DataGridColBuilder<TItem, TProperty> HasFilterFormatter<TFilter>(object? editorArgs = null)
            where TFilter : Filters.DataGridCellFilterBase
        {
            return HasFilterFormatter(typeof(TFilter), editorArgs);
        }

        public new DataGridColBuilder<TItem, TProperty> HasAutoEditor()
        {
            base.HasAutoEditor();
            return this;
        }
    }
}
