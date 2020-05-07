using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public class DataGridRowBuilder<TItem>
       where TItem : class
    {
        protected DataGridRowBuilder<TItem> LastFlow { get; }

        public DataGridBuilder<TItem> DataGridBuilder { get; }

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

        public DataGridRowBuilder<TItem> HasForcedRefresh(bool? value = true)
        {
            AddAction(rowInfo => rowInfo.ForceRefresh = value);

            return this;
        }

        public DataGridRowBuilder<TItem> HasCellValueChangedCallback(Func<DataGridSelectedCellInfo<TItem>, Task>? onCellValueChanged)
        {
            AddAction(rowInfo => rowInfo.OnCellValueChanged = onCellValueChanged);

            return this;
        }




        #region property groups

        public DataGridPropertyGroupBuilder<TItem> Properties<TProperty1>(Expression<Func<TItem, TProperty1>> property1)
        {
            return new DataGridPropertyGroupBuilder<TItem>(new List<DataGridCellBuilderGeneric<TItem>>()
            {
                 Property(property1)
            });
        }

        public DataGridPropertyGroupBuilder<TItem> Properties<
            TProperty1,
            TProperty2>(
            Expression<Func<TItem, TProperty1>> property1,
            Expression<Func<TItem, TProperty2>> property2)
        {
            return new DataGridPropertyGroupBuilder<TItem>(new List<DataGridCellBuilderGeneric<TItem>>()
            {
                 Property(property1),
                 Property(property2),
            });
        }

        public DataGridPropertyGroupBuilder<TItem> Properties<
            TProperty1,
            TProperty2,
            TProperty3>(
            Expression<Func<TItem, TProperty1>> property1,
            Expression<Func<TItem, TProperty2>> property2,
            Expression<Func<TItem, TProperty3>> property3)
        {
            return new DataGridPropertyGroupBuilder<TItem>(new List<DataGridCellBuilderGeneric<TItem>>()
            {
                 Property(property1),
                 Property(property2),
                 Property(property3),
            });
        }

        public DataGridPropertyGroupBuilder<TItem> Properties<
            TProperty1,
            TProperty2,
            TProperty3,
            TProperty4>(
            Expression<Func<TItem, TProperty1>> property1,
            Expression<Func<TItem, TProperty2>> property2,
            Expression<Func<TItem, TProperty3>> property3,
            Expression<Func<TItem, TProperty4>> property4)
        {
            return new DataGridPropertyGroupBuilder<TItem>(new List<DataGridCellBuilderGeneric<TItem>>()
            {
                 Property(property1),
                 Property(property2),
                 Property(property3),
                 Property(property4),
            });
        }

        public DataGridPropertyGroupBuilder<TItem> Properties<
            TProperty1,
            TProperty2,
            TProperty3,
            TProperty4,
            TProperty5>(
            Expression<Func<TItem, TProperty1>> property1,
            Expression<Func<TItem, TProperty2>> property2,
            Expression<Func<TItem, TProperty3>> property3,
            Expression<Func<TItem, TProperty4>> property4,
            Expression<Func<TItem, TProperty5>> property5)
        {
            return new DataGridPropertyGroupBuilder<TItem>(new List<DataGridCellBuilderGeneric<TItem>>()
            {
                 Property(property1),
                 Property(property2),
                 Property(property3),
                 Property(property4),
                 Property(property5),
            });
        }

        public DataGridPropertyGroupBuilder<TItem> Properties<
            TProperty1,
            TProperty2,
            TProperty3,
            TProperty4,
            TProperty5,
            TProperty6>(
            Expression<Func<TItem, TProperty1>> property1,
            Expression<Func<TItem, TProperty2>> property2,
            Expression<Func<TItem, TProperty3>> property3,
            Expression<Func<TItem, TProperty4>> property4,
            Expression<Func<TItem, TProperty5>> property5,
            Expression<Func<TItem, TProperty6>> property6)
        {
            return new DataGridPropertyGroupBuilder<TItem>(new List<DataGridCellBuilderGeneric<TItem>>()
            {
                 Property(property1),
                 Property(property2),
                 Property(property3),
                 Property(property4),
                 Property(property5),
                 Property(property6),
            });
        }

        public DataGridPropertyGroupBuilder<TItem> Properties<
            TProperty1,
            TProperty2,
            TProperty3,
            TProperty4,
            TProperty5,
            TProperty6,
            TProperty7>(
            Expression<Func<TItem, TProperty1>> property1,
            Expression<Func<TItem, TProperty2>> property2,
            Expression<Func<TItem, TProperty3>> property3,
            Expression<Func<TItem, TProperty4>> property4,
            Expression<Func<TItem, TProperty5>> property5,
            Expression<Func<TItem, TProperty6>> property6,
            Expression<Func<TItem, TProperty7>> property7)
        {
            return new DataGridPropertyGroupBuilder<TItem>(new List<DataGridCellBuilderGeneric<TItem>>()
            {
                 Property(property1),
                 Property(property2),
                 Property(property3),
                 Property(property4),
                 Property(property5),
                 Property(property6),
                 Property(property7),
            });
        }

        public DataGridPropertyGroupBuilder<TItem> Properties<
            TProperty1,
            TProperty2,
            TProperty3,
            TProperty4,
            TProperty5,
            TProperty6,
            TProperty7,
            TProperty8>(
            Expression<Func<TItem, TProperty1>> property1,
            Expression<Func<TItem, TProperty2>> property2,
            Expression<Func<TItem, TProperty3>> property3,
            Expression<Func<TItem, TProperty4>> property4,
            Expression<Func<TItem, TProperty5>> property5,
            Expression<Func<TItem, TProperty6>> property6,
            Expression<Func<TItem, TProperty7>> property7,
            Expression<Func<TItem, TProperty8>> property8)
        {
            return new DataGridPropertyGroupBuilder<TItem>(new List<DataGridCellBuilderGeneric<TItem>>()
            {
                 Property(property1),
                 Property(property2),
                 Property(property3),
                 Property(property4),
                 Property(property5),
                 Property(property6),
                 Property(property7),
                 Property(property8),
            });
        }

        #endregion
    }
}
