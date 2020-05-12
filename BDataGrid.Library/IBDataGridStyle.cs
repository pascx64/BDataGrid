using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public interface IBDataGridStyle
    {
        string TableClass { get; set; }

        string RowClass { get; set; }

        string CellClass { get; set; }

        string HeaderClass { get; set; }

        string HeaderRowClass { get; set; }

        string HeaderSortedAscendingClass { get; set; }

        string HeaderSortedDescendingClass { get; set; }

        string SelectedCell { get; set; }

        string PaginationDivClass { get; set; }

        string PaginationPageNumberClass { get; set; }

        string ExportExcelDivClass { get; set; }

        string ExportExcelAClass { get; set; }

        string ExportExcelIconClass { get; set; }

        string PaginationLeftIcon { get; set; }

        string PaginationRightIcon { get; set; }

        string? PopupInitializationJavascriptFunction { get; set; }

    }
}
