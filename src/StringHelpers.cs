using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenProject.ASH;

internal static class StringHelpers
{
    internal static string BytesToString(long byteCount)
    {
        string[] suffix = { "B", "K", "M", "G", "T", "P", "E" }; // long runs out around EB

        if (byteCount == 0)
            return "0" + suffix[0];

        var bytes = Math.Abs(byteCount);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1000)));
        var num = Math.Round(bytes / Math.Pow(1000, place), 1);

        return (Math.Sign(byteCount) * num) + suffix[place];
    }
}
