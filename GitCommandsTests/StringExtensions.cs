using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommandsTests
{
    public static class StringExtensions
    {
        public static string[] SplitLines(this string text)
        {
            return text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        }
    }
}
