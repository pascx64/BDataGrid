using BDataGrid.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Pages
{
    public partial class Index
    {
        public class DataItem
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public float Value { get; set; }

            public bool IsGroup { get; set; }

            public bool IsFilteredOut { get; set; }
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
                    .HasHeaderText("Value");

            builder
                .If(item => item.IsGroup)
                    .HasBackgroundColor(System.Drawing.Color.Gray)
                    .Property(p => p.Description)
                        .HasColSpan(2)
                .Else()
                    .HasBackgroundColor(System.Drawing.Color.Green)
                    .Property(p => p.Value)
                        .Formatter(item => item.Value + "$");
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();


            Datas = new List<DataItem>()
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
                }
            };
        }
    }
}
