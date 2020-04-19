using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BDataGrid.Library
{
    internal static class Utility
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
}
