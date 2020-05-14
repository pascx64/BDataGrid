using System;

namespace BDataGrid.Library
{
    public class DataGridRowBuilderConditional<TItem> : DataGridRowBuilder<TItem>
        where TItem : class
    {
        private Func<TItem, bool> Condition { get; set; }
        public DataGridRowBuilder<TItem>? Alternative { get; set; }

        public DataGridRowBuilderConditional(DataGridRowBuilder<TItem> lastFlow, Func<TItem, bool> condition) : base(lastFlow.DataGridBuilder, lastFlow)
        {
            Condition = condition;
        }

        public bool ExecuteConditional(DataGridRowInfo<TItem> rowInfo, TItem item)
        {
            if (Condition(item))
                return ExecuteActions(rowInfo, item);
            else if (Alternative is DataGridRowBuilderConditional<TItem> otherConditional)
                return otherConditional.ExecuteConditional(rowInfo, item);
            else if (Alternative != null)
                return Alternative.ExecuteActions(rowInfo, item);

            return false;
        }

        public override DataGridRowBuilder<TItem> ElseIf(Func<TItem, bool> condition)
        {
            var builder = new DataGridRowBuilderConditional<TItem>(LastFlow, condition);
            Alternative = builder;

            return builder;
        }
        public override DataGridRowBuilder<TItem> Else()
        {
            Alternative = new DataGridRowBuilder<TItem>(LastFlow.DataGridBuilder, LastFlow);

            return Alternative;
        }

        public override DataGridRowBuilder<TItem> EndIf()
        {
            return LastFlow;
        }
    }
}
