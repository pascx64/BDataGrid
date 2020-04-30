using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    public static class Utility
    {
        public static async Task FocusAsync(this ElementReference elementRef, IJSRuntime jsRuntime, string? additionalSelector = null)
        {
            await jsRuntime.InvokeVoidAsync("BDataGrid.focus", elementRef, additionalSelector);
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, T obj)
        {
            int i = 0;
            foreach (var item in enumerable)
            {
                if (item == null)
                {
                    if (obj == null)
                        return i;
                }
                else if (item.Equals(obj))
                    return i;
                ++i;
            }
            return -1;
        }
    }


    internal class DescendingComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var c = y.CompareTo(x);
            if (c == 0)
                return -1;
            return c;
        }
    }
    internal class AscendingComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var c = x.CompareTo(y);
            if (c == 0)
                return 1;
            return c;
        }
    }
}
