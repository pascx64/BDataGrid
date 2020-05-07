using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BDataGrid.Library
{
    public class DataGridPropertyGroupBuilder<TItem> : DataGridCellBuilderGeneric<TItem>
        where TItem : class
    {
        private readonly List<DataGridCellBuilderGeneric<TItem>> Properties = new List<DataGridCellBuilderGeneric<TItem>>();

        internal DataGridPropertyGroupBuilder(List<DataGridCellBuilderGeneric<TItem>> properties)
        {
            Properties = properties;
        }

        public DataGridPropertyGroupBuilder<TItem> And<TProperty1>(Expression<Func<TItem, TProperty1>> property1)
        {
            Properties.Add(GetProperty(property1));
            return this;
        }

        public override void AddAction(Action<DataGridRowInfo<TItem>, DataGridCellInfo<TItem>> action)
        {
            foreach (var property in Properties)
                property.AddAction(action);
        }

        public override void ExecuteActions(DataGridRowInfo<TItem> rowInfo)
        {
            foreach (var property in Properties)
                property.ExecuteActions(rowInfo);
        }

        internal override DataGridCellBuilder<TItem, TProperty> GetProperty<TProperty>(Expression<Func<TItem, TProperty>> selector)
        {
            return Properties.First().GetProperty(selector);
        }

        internal override DataGridCellBuilderGeneric<TItem> HasAutoEditorGeneric()
        {
            foreach (var property in Properties)
                property.HasAutoEditorGeneric();

            return this;
        }
    }
}
