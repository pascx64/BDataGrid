using BDataGrid.Library;
using BDataGrid.Library.Formatters;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Example.Pages
{
    public partial class Index : ComponentBase
    {
        public class DataItem
        {
            public bool Checkbox { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            [DataGridColumnInfo(AllowEdit = true)]
            public float Value { get; set; }

            public string? ActionHere { get; set; }

            public bool IsGroup { get; set; }

            public bool IsFilteredOut { get; set; }

            public bool IsTotal { get; set; }
        }

        private List<DataItem> Datas { get; set; }

        public void ConfigureDatagrid(DataGridBuilder<DataItem> builder)
        {
            builder
                .Property(p => p.Checkbox)
                    .HasHeaderText("CB")
                    .HasWidth("50px")
                    .HasAutoEditor()
                    .HasFixedCol()
                    .HasBackgroundColor(System.Drawing.Color.White)
                .Property(p => p.ActionHere)
                    .HasWidth("150px")
                .Property(p => p.Name)
                    .HasHeaderText("Name")
                    .HasWidth("250px")
                .Property(p => p.Description)
                    .HasHeaderText("Description")
                    .HasWidth("500px")
                .Property(p => p.Value)
                    .HasHeaderText("Value")
                    .HasWidth("100px")
                    .HasAppendedText("$")
                    .HasAutoEditor();

            builder.HasFilterRow();

            builder
                .If(item => !item.IsTotal)
                    .If(item => !item.IsGroup)
                        .Property(p => p.Description)
                            .HasTextEditor()
                    .ElseRow()
                        .HasClass("active bold")
                        .IsReadOnly()
                        .Property(p => p.Description)
                            .HasColSpan(2)
                    .EndIf()
                    .If(item => item.Value % 2 == 0)
                        .Property(p => p.Value)
                            .If(item => item.Value == 4)
                                .HasClass("positive")
                            .Else()
                                .HasBackgroundColor("#fcd1d1") // error/red color
                            .EndIf()
                        .Property(p => p.Description)
                            .HasClass("warning")
                        .Property( p => p.Checkbox )
                            .IsNotReadOnly()
                    .ElseRow()
                        .Property(p => p.Description)
                            .HasClass("positive");

            builder
                .If(x => x.IsTotal)
                    .HasFooterLocation()
                    .Property(p => p.Value)
                        .HasFormatter(_ => builder.FilteredItems.Sum(x => x.Item.Value).ToString("0.00"));

            builder
                .If(x => !string.IsNullOrEmpty(x.ActionHere))
                    .Property(p => p.ActionHere)
                        .HasButtonFormatter(() =>
                        {
                            Console.WriteLine("Clicked!");
                        });
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();


            Datas = Enumerable.Range(0, 9999999).SelectMany(x => new List<DataItem>()
            {
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    IsGroup = true,
                    Name = "group 1",
                    Description = "Group de fou",
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 1",
                    Description = "wouhouuu here here here",
                    ActionHere = "test",
                    Value = 1
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 2",
                    Description = "sadadawd",
                    Value = 2
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    IsGroup = true,
                    Name = "group 2",
                    Description = "Group de fou adwad",
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 1",
                    Description = "wouhouuu here here here",
                    Value = 3
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 2",
                    Description = "sadadawd",
                    ActionHere = "test 2",
                    Value = 4
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 3",
                    Description = "sadadawd",
                    Value = 5
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 2",
                    Description = "sadadawd",
                    Value = 6
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    IsGroup = true,
                    Name = "group 3",
                    Description = "Group de fou adwad",
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 1",
                    Description = "wouhouuu here here here",
                    Value = 7
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 2",
                    Description = "sadadawd",
                    Value = 8
                },
                new DataItem()
                {
                    Checkbox = new Random().Next() % 2 == 0,
                    Name = "sub item 3",
                    Description = "sadadawd",
                    Value = 9
                }
            }).Take(2).Append(new DataItem()
            {
                IsTotal = true
            }).ToList();
        }
    }
}
