using System;
using System.Text.RegularExpressions;

namespace GbaMonoGame;

public static class StringExtensions
{
    public static string sprintf(this string input, params object[] inpVars)
    {
        int i = 0;
        input = Regex.Replace(input, "%.", _ => "{" + i++ + "}");
        return String.Format(input, inpVars);
    }
}