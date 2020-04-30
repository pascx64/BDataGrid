using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public class DataGridCellBuilder<TItem, TProperty>
        where TItem : class
    {
        protected Expression<Func<TItem, TProperty>> Selector { get; set; }

        public string PropertyName { get; private set; }

        public DataGridRowBuilder<TItem> DataGridRowBuilder { get; }

        public readonly Func<TItem, TProperty> SelectorFunc;

        public readonly Action<TItem, object?> SetFunc;

        public readonly Func<object?, TProperty> ConvertToProperty;


        public DataGridCellBuilder(Expression<Func<TItem, TProperty>> selector, DataGridRowBuilder<TItem> dataGridRowBuilder)
        {
            Selector = selector;
            DataGridRowBuilder = dataGridRowBuilder;

            SelectorFunc = Selector.Compile();

            if (Selector.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)Selector.Body;

                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                {
                    var member = memberExpression.Member;
                    if (member is PropertyInfo memberProperty)
                        SetFunc = memberProperty.SetValue;
                    else if (member is FieldInfo memberField)
                        SetFunc = memberField.SetValue;
                    else
                        throw new Exception("Unable to determine member access :" + selector.ToString());

                    PropertyName = member.Name;
                }
                else
                    throw new Exception("Unable to determine member access :" + selector.ToString());
            }
            else
                throw new Exception("Unable to determine member access :" + selector.ToString());


            ConvertToProperty = ConvertToPropertyValue;
        }
        private readonly Queue<Action<DataGridRowInfo<TItem>, DataGridCellInfo<TItem>>> Actions = new Queue<Action<DataGridRowInfo<TItem>, DataGridCellInfo<TItem>>>();

        public DataGridCellBuilder<TItem, TProperty> AddAction(Action<DataGridRowInfo<TItem>, DataGridCellInfo<TItem>> action)
        {
            Actions.Enqueue(action);
            return this;
        }

        public virtual void ExecuteActions(DataGridRowInfo<TItem> rowInfo)
        {
            if (rowInfo.Cells == null)
                rowInfo.Cells = new Dictionary<string, DataGridCellInfo<TItem>>();

            if (!rowInfo.Cells.TryGetValue(PropertyName, out var cellInfo))
            {
                cellInfo = rowInfo.Cells[PropertyName] = new DataGridCellInfo<TItem>();
                if (typeof(TProperty) == typeof(bool) || typeof(TProperty) == typeof(bool?))
                    cellInfo.Formatter = GetFormatter(typeof(Formatters.DataGridFormatter_CheckBox));
            }

            foreach (var action in Actions)
                action(rowInfo, cellInfo);
        }

        public DataGridCellBuilder<TItem, TProperty2> Property<TProperty2>(Expression<Func<TItem, TProperty2>> selector)
        {
            return DataGridRowBuilder.Property(selector);
        }

        public DataGridCellBuilder<TItem, TProperty> If(Func<TItem, bool> condition)
        {
            var conditionalRowBuilder = DataGridRowBuilder.If(condition);

            return conditionalRowBuilder.Property(Selector);
        }
        public DataGridRowBuilder<TItem> IfRow(Func<TItem, bool> condition)
        {
            return DataGridRowBuilder.If(condition);
        }

        public DataGridRowBuilder<TItem> ElseIfRow(Func<TItem, bool> condition)
        {
            return DataGridRowBuilder.ElseIf(condition);

        }

        public DataGridCellBuilder<TItem, TProperty> ElseIf(Func<TItem, bool> condition)
        {
            var conditionalRowBuilder = DataGridRowBuilder.ElseIf(condition);

            return conditionalRowBuilder.Property(Selector);
        }

        public DataGridCellBuilder<TItem, TProperty> Else()
        {
            var conditionalRowBuilder = DataGridRowBuilder.Else();

            return conditionalRowBuilder.Property(Selector);
        }
        public DataGridRowBuilder<TItem> ElseRow()
        {
            return DataGridRowBuilder.Else();
        }

        public DataGridRowBuilder<TItem> EndIf()
        {
            return DataGridRowBuilder.EndIf();
        }

        public DataGridRowBuilder<TItem> Row()
        {
            return DataGridRowBuilder;
        }

        public DataGridCellBuilder<TItem, TProperty> HasColSpan(int colspan)
        {
            return AddAction((_, cell) => cell.ColSpan = colspan);
        }

        public DataGridCellBuilder<TItem, TProperty> HasClass(string classes, bool overrideExisting = true)
        {
            return AddAction((_, cell) => cell.Classes = overrideExisting ? classes : ((cell.Classes ?? "") + " " + classes).Trim());
        }

        public DataGridCellBuilder<TItem, TProperty> HasBackgroundColor(System.Drawing.Color color)
        {
            return AddAction((_, cell) => cell.BackgroundColor = color);
        }

        public DataGridCellBuilder<TItem, TProperty> HasBackgroundColor(string htmlColor)
        {
            return AddAction((_, cell) => cell.BackgroundColor = System.Drawing.ColorTranslator.FromHtml(htmlColor));
        }

        public DataGridCellBuilder<TItem, TProperty> IsReadOnly()
        {
            return AddAction((_, cell) => cell.IsReadOnly = true);
        }

        public DataGridCellBuilder<TItem, TProperty> IsNotReadOnly()
        {
            return AddAction((_, cell) => cell.IsReadOnly = false);
        }

        public DataGridCellBuilder<TItem, TProperty> IsEditable()
        {
            return AddAction((_, cell) => cell.IsReadOnly = false);
        }

        public DataGridCellBuilder<TItem, TProperty> ReplaceWith(string content = "")
        {
            return AddAction((_, cell) => cell.FormatterString = _ => content);
        }

        public DataGridCellBuilder<TItem, TProperty> HasFormatter(Func<TItem, string> formatter)
        {
            return AddAction((_, cell) => cell.FormatterString = formatter);
        }

        public DataGridCellBuilder<TItem, TProperty> HasAutoEditor()
        {
            var type = typeof(TProperty);
            var realType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                realType = Nullable.GetUnderlyingType(type);


            if (realType == typeof(string))
                return HasEditor<Editors.BDataGridEditor_Text>();
            if (realType == typeof(float) || realType == typeof(double) || realType == typeof(int) || realType == typeof(uint) || realType == typeof(decimal)
                || realType == typeof(short) || realType == typeof(byte) || realType == typeof(long) || realType == typeof(ulong)
                || realType == typeof(bool))
            {
                HasEditorValueConversion(str =>
                {
                    try
                    {
                        var value = ConvertToPropertyValue(str);
                        return new EditorValueConversionResult(value);
                    }
                    catch (Exception)
                    {
                        return new EditorValueConversionResult("Erreur de conversion");
                    }
                });
                if (realType != typeof(bool))
                    return HasEditor<Editors.BDataGridEditor_Text>();

                return this;
            }


            throw new Exception("Unknowned type for automatic editor: " + type);
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor(Func<TItem, RenderFragment<DataGridEditorArgs>> renderFragmentProvider)
        {
            return AddAction((row, cell) =>
            {
                if (cell.EditorInfo == null)
                    cell.EditorInfo = new DataGridEditorInfo<TItem>();
                cell.EditorInfo.RenderFragmentProvider = renderFragmentProvider;
            });
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor(Type editorType, Func<TItem, object>? argsProvider = null)
        {
            return HasEditor(item =>
            {
                return args =>
                {
                    return builder =>
                    {
                        builder.OpenComponent(0, editorType);
                        builder.AddAttribute(1, "Args", args);
                        if (argsProvider != null)
                        {
                            var editorArgs = argsProvider(item);
                            int sequence = 1;
                            foreach (var property in editorArgs.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                                builder.AddAttribute(++sequence, property.Name, property.GetValue(editorArgs));
                        }
                        builder.CloseComponent();
                    };
                };
            });
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor<TEditor>()
            where TEditor : Editors.BDataGridEditorBase
        {
            return HasEditor(typeof(TEditor), null);
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor<TEditor>(object args)
            where TEditor : Editors.BDataGridEditorBase
        {
            return HasEditor(typeof(TEditor), _ => args);
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditor<TEditor>(Func<TItem, object> argsProvider)
            where TEditor : Editors.BDataGridEditorBase
        {
            return HasEditor(typeof(TEditor), argsProvider);
        }

        public DataGridCellBuilder<TItem, TProperty> HasTextEditor()
        {
            return HasEditor<Editors.BDataGridEditor_Text>();
        }

        public DataGridCellBuilder<TItem, TProperty> HasAppendedText(string append)
        {
            return AddAction((row, cell) => cell.Append = append);
        }

        public DataGridCellBuilder<TItem, TProperty> HasFormatter(RenderFragment<DataGridFormatterArgs> renderFragmentProvider)
        {
            return AddAction((row, cell) =>
            {
                cell.Formatter = renderFragmentProvider;
            });
        }

        private RenderFragment<DataGridFormatterArgs> GetFormatter(Type editorType, Func<TItem, object>? argsProvider = null)
        {
            Func<TItem, object?, DataGridColInfo<TItem>, DataGridRowInfo<TItem>, DataGridCellInfo<TItem>, Task<DataGridAcceptChangesResult>> callback = (item, value, col, row, cell) =>
            {
                return DataGridRowBuilder.DataGridBuilder.TryAcceptChanges(item, value, col, row, cell);
            };

            return args =>
            {
                return builder =>
                {
                    builder.OpenComponent(0, editorType);
                    builder.AddAttribute(1, "Args", args);
                    if (argsProvider != null)
                    {
                        var editorArgs = argsProvider((TItem)args.Item);
                        int sequence = 4;
                        foreach (var property in editorArgs.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                            builder.AddAttribute(++sequence, property.Name, property.GetValue(editorArgs));
                    }
                    builder.CloseComponent();
                };
            };
        }

        public DataGridCellBuilder<TItem, TProperty> HasFormatter(Type editorType, Func<TItem, object>? argsProvider = null)
        {
            return AddAction((row, cell) => cell.Formatter = GetFormatter(editorType, argsProvider));
        }

        public DataGridCellBuilder<TItem, TProperty> HasFormatter<TFormatter>(Func<TItem, object>? argsProvider = null)
            where TFormatter : Formatters.DataGridFormatter
        {
            return HasFormatter(typeof(TFormatter), argsProvider);
        }

        public DataGridCellBuilder<TItem, TProperty> HasButtonFormatter(Func<TItem, Task> action)
        {
            return HasFormatter<Formatters.DataGridFormatter_Button>(item => new
            {
                Callback = (Func<object, Task>)(obj => action((TItem)obj))
            });
        }

        public DataGridCellBuilder<TItem, TProperty> HasButtonFormatter(Func<Task> action)
        {
            return HasButtonFormatter(_ => action());
        }

        public DataGridCellBuilder<TItem, TProperty> HasButtonFormatter(Action action)
        {
            return HasButtonFormatter(_ =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        public DataGridCellBuilder<TItem, TProperty> HasValidator(Func<TItem, TProperty, ValidationResult> validator)
        {
            return AddAction((_, cell) =>
            {
                var previousValidator = cell.Validator;
                cell.Validator = (item, obj) =>
                {
                    if (previousValidator != null)
                    {
                        var previousResult = previousValidator(item, obj);
                        if (!previousResult.IsValid)
                            return previousResult;
                    }

                    return validator(item, ConvertToProperty(obj));
                };
            });
        }

        public DataGridCellBuilder<TItem, TProperty> HasValidator(Func<TProperty, ValidationResult> validator)
        {
            return HasValidator((_, cell) => validator(cell));
        }

        public DataGridCellBuilder<TItem, TProperty> HasNewValidator(Func<TItem, TProperty, ValidationResult>? validator)
        {
            return AddAction((_, cell) =>
            {
                cell.Validator = validator == null ? null : (Func<TItem, object?, ValidationResult>?)((item, obj) => validator(item, ConvertToProperty(obj)));
            });
        }

        public DataGridCellBuilder<TItem, TProperty> HasNewValidator(Func<TProperty, ValidationResult>? validator)
        {
            return HasValidator((_, cell) => validator?.Invoke(cell) ?? new ValidationResult(true));
        }

        public DataGridCellBuilder<TItem, TProperty> HasCellValueChangedCallback(Func<DataGridSelectedCellInfo<TItem>, Task>? onCellValueChanged)
        {
            AddAction((_, cell) => cell.OnCellValueChanged = onCellValueChanged);

            return this;
        }

        public DataGridCellBuilder<TItem, TProperty> HasEditorValueConversion(Func<object?, EditorValueConversionResult>? editorValueConversion)
        {
            AddAction((_, cell) => cell.EditorValueConversion = editorValueConversion);

            return this;
        }

        private static TProperty ConvertToPropertyValue(object? obj)
        {
            var type = typeof(TProperty);
            var realType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                realType = Nullable.GetUnderlyingType(type);

            if (obj == null)
            {
                if (type == realType)
                {
                    if (!type.IsClass)
                        throw new Exception("null cannot be converted to type:" + type);
#pragma warning disable CS8603 // Possible null reference return.
                    return default;
#pragma warning restore CS8603 // Possible null reference return.
                }
                else
                    return (TProperty)Activator.CreateInstance(type);
            }

            object returnValue;
            if (realType == obj.GetType())
                returnValue = obj;
            else if (realType == typeof(string))
                returnValue = obj.ToString();
            else if (realType == typeof(float) || realType == typeof(double) || realType == typeof(int) || realType == typeof(uint) || realType == typeof(decimal)
                || realType == typeof(short) || realType == typeof(byte))
                returnValue = Convert.ChangeType(obj, realType);
            else if (realType == typeof(bool))
            {
                if (obj is bool b)
                    returnValue = b;
                else if (obj is string str)
                    returnValue = Convert.ChangeType(obj, realType);
                else
                    throw new Exception("Cannot convert to bool from type:" + type);
            }
            else
                throw new Exception("Cannot convert object to type:" + type);

            if (realType != type)
                returnValue = Activator.CreateInstance(type, returnValue);

            return (TProperty)returnValue;
        }
    }
}
