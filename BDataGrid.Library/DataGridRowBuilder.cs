using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BDataGrid.Library
{
    public class DataGridRowBuilder<TItem>
       where TItem : class
    {
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

        public DataGridRowBuilder<TItem> If(Func<TItem, bool> condition)
        {
            var conditional = new DataGridRowBuilderConditional<TItem>(condition);

            AddAction(conditional.ExecuteConditional);

            return conditional;
        }

        public DataGridRowBuilder<TItem> ElseIf(Func<TItem, bool> condition)
        {
            var currentConditional = this as DataGridRowBuilderConditional<TItem> ?? throw new InvalidCastException("No condition");
            var builder = new DataGridRowBuilderConditional<TItem>(condition);
            currentConditional.Alternative = builder;
            return builder;
        }

        public DataGridRowBuilder<TItem> Else()
        {
            var currentConditional = this as DataGridRowBuilderConditional<TItem> ?? throw new InvalidCastException("No condition");
            return currentConditional.Alternative = new DataGridRowBuilder<TItem>();
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

        public DataGridRowBuilder<TItem> HasClass(string classes, bool overrideExisting)
        {
            AddAction(rowInfo => rowInfo.Classes = overrideExisting ? classes : (rowInfo.Classes ?? "") + " " + classes);

            return this;
        }

    }
}
