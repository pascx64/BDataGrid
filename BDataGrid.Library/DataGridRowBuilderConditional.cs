using System;

namespace BDataGrid.Library
{
    public class DataGridRowBuilderConditional<TItem> : DataGridRowBuilder<TItem>
        where TItem : class
    {
        private Func<TItem, bool> Condition { get; set; }
        public DataGridRowBuilder<TItem>? Alternative { get; set; }

        public DataGridRowBuilderConditional(Func<TItem, bool> condition) : base()
        {
            Condition = condition;
        }

        public bool ExecuteConditional(DataGridRowInfo<TItem> rowInfo, TItem item)
        {
            if (Condition(item))
                return ExecuteActions(rowInfo, item);
            else if (Alternative != null)
                return Alternative.ExecuteActions(rowInfo, item);
            return false;
        }

    }
}
