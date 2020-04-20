using BDataGrid.Library;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Example.Pages
{
    public partial class Index : ComponentBase
    {
        public class DataItem
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public float Value { get; set; }

            public bool IsGroup { get; set; }

            public bool IsFilteredOut { get; set; }

            public bool IsTotal { get; set; }
        }

        private List<DataItem> Datas { get; set; }

        public void ConfigureDatagrid(DataGridBuilder<DataItem> builder)
        {
            builder
                .Property(p => p.Name)
                    .HasHeaderText("Name")
                .Property(p => p.Description)
                    .HasHeaderText("Description")
                .Property(p => p.Value)
                    .HasHeaderText("Value")
                    .HasAppendedText("$");

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
                    .ElseRow()
                        .Property(p => p.Description)
                            .HasClass("positive");

            builder
                .If(x => x.IsTotal)
                    .HasFooterLocation()
                    .Property(p => p.Value)
                        .HasFormatter(_ => builder.FilteredItems.Sum(x => x.Item.Value).ToString("0.00"));
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();


            Datas = Enumerable.Range(0, 1000).SelectMany(x => new List<DataItem>()
            {
                new DataItem()
                {
                    IsGroup = true,
                    Name = "group 1",
                    Description = "Group de fou",
                },
                new DataItem()
                {
                    Name = "sub item 1",
                    Description = "wouhouuu here here here",
                    Value = 1
                },
                new DataItem()
                {
                    Name = "sub item 2",
                    Description = "sadadawd",
                    Value = 2
                },
                new DataItem()
                {
                    IsGroup = true,
                    Name = "group 2",
                    Description = "Group de fou adwad",
                },
                new DataItem()
                {
                    Name = "sub item 1",
                    Description = "wouhouuu here here here",
                    Value = 3
                },
                new DataItem()
                {
                    Name = "sub item 2",
                    Description = "sadadawd",
                    Value = 4
                },
                new DataItem()
                {
                    Name = "sub item 3",
                    Description = "sadadawd",
                    Value = 5
                },
                new DataItem()
                {
                    Name = "sub item 2",
                    Description = "sadadawd",
                    Value = 6
                },
                new DataItem()
                {
                    IsGroup = true,
                    Name = "group 3",
                    Description = "Group de fou adwad",
                },
                new DataItem()
                {
                    Name = "sub item 1",
                    Description = "wouhouuu here here here",
                    Value = 7
                },
                new DataItem()
                {
                    Name = "sub item 2",
                    Description = "sadadawd",
                    Value = 8
                },
                new DataItem()
                {
                    Name = "sub item 3",
                    Description = "sadadawd",
                    Value = 9
                }
            }).Append(new DataItem()
            {
                IsTotal = true
            }).ToList();
        }
    }
}
