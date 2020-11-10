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
            public string Description2 { get; set; } = new Random().Next().ToString();
            public string Description3 { get; set; } = new Random().Next().ToString();
            public string Description4 { get; set; } = new Random().Next().ToString();
            public string Description5 { get; set; } = new Random().Next().ToString();
            public string Description6 { get; set; } = new Random().Next().ToString();
            public string Description7 { get; set; } = new Random().Next().ToString();
            public string Description8 { get; set; } = new Random().Next().ToString();
            public string Description9 { get; set; } = new Random().Next().ToString();
            public string Description12 { get; set; } = new Random().Next().ToString();
            public string Description13 { get; set; } = new Random().Next().ToString();
            public string Description14 { get; set; } = new Random().Next().ToString();
            public string Description15 { get; set; } = new Random().Next().ToString();
            public string Description16 { get; set; } = new Random().Next().ToString();
            public string Description17 { get; set; } = new Random().Next().ToString();
            public string Description18 { get; set; } = new Random().Next().ToString();
            public string Description19 { get; set; } = new Random().Next().ToString();
            public string Description22 { get; set; } = new Random().Next().ToString();
            public string Description23 { get; set; } = new Random().Next().ToString();
            public string Description24 { get; set; } = new Random().Next().ToString();
            public string Description25 { get; set; } = new Random().Next().ToString();
            public string Description26 { get; set; } = new Random().Next().ToString();
            public string Description27 { get; set; } = new Random().Next().ToString();
            public string Description28 { get; set; } = new Random().Next().ToString();
            public string Description29 { get; set; } = new Random().Next().ToString();

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
                    .HasBackgroundColor(Color.White)
                .Property(p => p.ActionHere)
                    .HasWidth("150px")
                    .HasPopup("Title here", "Content here")
                .Property(p => p.Name)
                    .HasHeaderText("Name")
                    .HasWidth("250px")
                .Property(p => p.Description)
                    .HasHeaderText("Description")
                    .HasWidth("500px")
                .Property(p => p.Description2).HasHeaderText("Description2")
                .Property(p => p.Description3).HasHeaderText("Description3")
                .Property(p => p.Description4).HasHeaderText("Description4")
                .Property(p => p.Description5).HasHeaderText("Description5")
                .Property(p => p.Description6).HasHeaderText("Description6")
                .Property(p => p.Description7).HasHeaderText("Description7")
                .Property(p => p.Description8).HasHeaderText("Description8")
                .Property(p => p.Description9).HasHeaderText("Description9")
                .Property(p => p.Description12).HasHeaderText("Description12")
                .Property(p => p.Description13).HasHeaderText("Description13")
                .Property(p => p.Description14).HasHeaderText("Description14")
                .Property(p => p.Description15).HasHeaderText("Description15")
                .Property(p => p.Description16).HasHeaderText("Description16")
                .Property(p => p.Description17).HasHeaderText("Description17")
                .Property(p => p.Description18).HasHeaderText("Description18")
                .Property(p => p.Description19).HasHeaderText("Description19")
                .Property(p => p.Description22).HasHeaderText("Description22")
                .Property(p => p.Description23).HasHeaderText("Description23")
                .Property(p => p.Description24).HasHeaderText("Description24")
                .Property(p => p.Description25).HasHeaderText("Description25")
                .Property(p => p.Description26).HasHeaderText("Description26")
                .Property(p => p.Description27).HasHeaderText("Description27")
                .Property(p => p.Description28).HasHeaderText("Description28")
                .Property(p => p.Description29).HasHeaderText("Description29")
                .Property(p => p.Value)
                    .HasHeaderText("Value")
                    .HasWidth("100px")
                    .HasAppendedText("$")
                    .HasAutoEditor()
                    .HasRightAlignedText();

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
                        .Property(p => p.Checkbox)
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
            }).Take(80100).Append(new DataItem()
            {
                IsTotal = true
            }).ToList();
        }
    }
}
