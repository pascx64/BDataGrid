﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{
    public class BDataGridStyleEmpty : IBDataGridStyle
    {
        public string TableClass { get; set; } = "";

        public string RowClass { get; set; } = "";

        public string CellClass { get; set; } = "";

        public string HeaderClass { get; set; } = "";

        public string HeaderRowClass { get; set; } = "";

        public string SelectedCell { get; set; } = "";

        public string PaginationDivClass { get; set; } = "";

        public string PaginationPageNumberClass { get; set; } = "";

        public string PaginationLeftIcon { get; set; } = "";

        public string PaginationRightIcon { get; set; } = "";

        public string HeaderSortedAscendingClass { get; set; } = "";

        public string HeaderSortedDescendingClass { get; set; } = "";

        public string ExportExcelDivClass { get; set; } = "";

        public string ExportExcelAClass { get; set; } = "";

        public string ExportExcelIconClass { get; set; } = "";

        public string? PopupInitializationJavascriptFunction { get; set; } = null;
    }
}
