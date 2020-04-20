using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BDataGrid.Library
{
    public class DataGridRowBuilder<TItem>
       where TItem : class
    {
        protected DataGridRowBuilder<TItem> LastFlow { get; }

        internal DataGridBuilder<TItem> DataGridBuilder { get; }

        public DataGridRowBuilder(DataGridBuilder<TItem>? dataGridBuilder, DataGridRowBuilder<TItem>? lastFlow)
        {
            DataGridBuilder = dataGridBuilder ?? (DataGridBuilder<TItem>)this;
            LastFlow = lastFlow ?? this;
        }
        #region constructor and generic methods

        protected Queue<KeyValuePair<Action<DataGridRowInfo<TItem>>?, Func<DataGridRowInfo<TItem>, TItem, bool>?>> Actions { get; } = new Queue<KeyValuePair<Action<DataGridRowInfo<TItem>>?, Func<DataGridRowInfo<TItem>, TItem, bool>?>>();

        public virtual void AddAction(Action<DataGridRowInfo<TItem>> action)
        {
            Actions.Enqueue(new KeyValuePair<Action<DataGridRowInfo<TItem>>?, Func<DataGridRowInfo<TItem>, TItem, bool>?>(action, null));
        }

        public void AddAction(Func<DataGridRowInfo<TItem>, TItem, bool> action)
        {
            Actions.Enqueue(new KeyValuePair<Action<DataGridRowInfo<TItem>>?, Func<DataGridRowInfo<TItem>, TItem, bool>?>(null, action));
        }

        public bool ExecuteActions(DataGridRowInfo<TItem> rowInfo, TItem item)
        {
            bool affected = false;
            foreach (var action in Actions)
            {
                if (action.Key != null)
                {
                    action.Key(rowInfo);
                    affected = true;
                }
                else if (action.Value != null)
                    affected |= action.Value(rowInfo, item);
            }

            return affected;
        }

        #endregion

        public DataGridCellBuilder<TItem, TProperty> Property<TProperty>(Expression<Func<TItem, TProperty>> selector)
        {
            var cellBuilder = new DataGridCellBuilder<TItem, TProperty>(selector, this);

            AddAction(cellBuilder.ExecuteActions);

            return cellBuilder;
        }

        public virtual DataGridRowBuilder<TItem> If(Func<TItem, bool> condition)
        {
            var conditional = new DataGridRowBuilderConditional<TItem>(this, condition);

            AddAction(conditional.ExecuteConditional);

            return conditional;
        }

        public virtual DataGridRowBuilder<TItem> ElseIf(Func<TItem, bool> condition)
        {
            throw new InvalidCastException("No condition");
        }

        public virtual DataGridRowBuilder<TItem> Else()
        {
            throw new InvalidCastException("No condition");
        }

        public virtual DataGridRowBuilder<TItem> EndIf()
        {
            return LastFlow;
        }

        public DataGridRowBuilder<TItem> HasBackgroundColor(System.Drawing.Color color)
        {
            AddAction(rowInfo => rowInfo.BackgroundColor = color);

            return this;
        }

        public DataGridRowBuilder<TItem> HasBackgroundColor(string htmlColor)
        {
            AddAction(rowInfo => rowInfo.BackgroundColor = System.Drawing.ColorTranslator.FromHtml(htmlColor));

            return this;
        }

        public DataGridRowBuilder<TItem> IsReadOnly(bool value = true)
        {
            AddAction(rowInfo => rowInfo.IsReadOnly = value);
            return this;
        }

        public DataGridRowBuilder<TItem> HasLocation(RowLocation location)
        {
            AddAction(rowInfo => rowInfo.RowLocation = location);
            return this;
        }

        public DataGridRowBuilder<TItem> HasFooterLocation()
        {
            return HasLocation(RowLocation.Footer);
        }

        public DataGridRowBuilder<TItem> HasHeaderLocation()
        {
            return HasLocation(RowLocation.Header);
        }

        public DataGridRowBuilder<TItem> HasClass(string classes, bool overrideExisting = false)
        {
            AddAction(rowInfo => rowInfo.Classes = overrideExisting ? classes : (rowInfo.Classes ?? "") + " " + classes);

            return this;
        }

    }
}
