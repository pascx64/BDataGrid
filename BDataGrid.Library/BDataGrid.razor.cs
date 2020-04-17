using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public partial class BDataGrid<TData>
        where TData : class
    {
        [Parameter]
        public string TableCssClass { get; set; } = "";

        [Parameter]
        public IReadOnlyList<TData> Datas { get; set; }
        
        [Parameter]
        public Action<DataGridBuilder<TData>> Configure { get; set; }

        private DataGridBuilder<TData> Builder { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (Builder == null)
            {
                Builder = new DataGridBuilder<TData>();
                Configure(Builder);
                Builder.Build(Datas);
            }
        }
    }
}
