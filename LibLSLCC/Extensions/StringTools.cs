﻿#region FileInfo

// 
// File: StringTools.cs
// 
// Author/Copyright:  Eric A. Blundell
// 
// Last Compile: 24/09/2015 @ 9:25 PM
// 
// Creation Date: 21/08/2015 @ 12:22 AM
// 
// 
// This file is part of LibLSLCC.
// LibLSLCC is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// LibLSLCC is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with LibLSLCC.  If not, see <http://www.gnu.org/licenses/>.
// 

#endregion

namespace LibLSLCC.Extensions
{
    public static class StringTools
    {
        /// <summary>
        ///     Gets the number of spaces required to match the length of the whitespace leading up to the first non-whitespace
        ///     character in a string (new line is not considered whitespace here).
        /// </summary>
        /// <param name="str">The string to consider</param>
        /// <param name="tabSize">The size of a tab character in spaces</param>
        /// <returns>
        ///     The number of space characters required to match the length of all the whitespace characters at the end of the
        ///     string (except newlines)
        /// </returns>
        public static int GetStringSpacesIndented(this string str, int tabSize = 4)
        {
            var columns = 0;

            foreach (var t in str)
            {
                if (char.IsWhiteSpace(t))
                {
                    if (t == '\t')
                    {
                        columns += 4;
                    }
                    else if (t == ' ')
                    {
                        columns++;
                    }
                }
                else
                {
                    break;
                }
            }
            return columns;
        }

        /// <summary>
        ///     Gets the number of spaces required to exactly match the length of a given string up to the first new line
        /// </summary>
        /// <param name="str">Input string to get the length in spaces of</param>
        /// <param name="tabSize">Tab size in spaces, defaults to 4</param>
        /// <returns>Number of spaces required to match the length of the string</returns>
        public static int GetStringSpacesEquivalent(this string str, int tabSize = 4)
        {
            if (str.Length == 0) return 0;

            var columns = 0;

            for (var index = 0; index < str.Length; index++)
            {
                var t = str[index];
                if (char.IsWhiteSpace(t))
                {
                    if (t == '\t')
                    {
                        columns += tabSize;
                    }
                    else if (t == ' ')
                    {
                        columns++;
                    }
                }
                else if (char.IsDigit(t) || char.IsLetter(t) || char.IsSymbol(t) || char.IsPunctuation(t))
                {
                    columns += 1;
                }
                else if (index + 1 < str.Length && char.IsHighSurrogate(t) && char.IsLowSurrogate(str[index + 1]))
                {
                    columns += 1;
                    index++;
                }
                else if (t == '\n')
                {
                    break;
                }
            }
            return columns;
        }

        /// <summary>
        ///     Creates a spacer string using tabs up until spaces are required for alignment.
        ///     Strings less than tabSize end up being only spaces.
        /// </summary>
        /// <param name="spaces">The number of spaces the spacer string should be equivalent to</param>
        /// <param name="tabSize">The size of a tab character in spaces, default value is 4</param>
        /// <returns>
        ///     A string consisting of leading tabs and possibly trailing spaces that is equivalent in length
        ///     to the number of spaces provided in the spaces parameter
        /// </returns>
        public static string CreateTabCorrectSpaceString(int spaces, int tabSize = 4)
        {
            var space = "";
            var actual = 0;
            for (var i = 0; i < (spaces/tabSize); i++)
            {
                space += '\t';
                actual += tabSize;
            }

            while (actual < spaces)
            {
                space += ' ';
                actual++;
            }


            return space;
        }

        public static string CreateRepeatingString(int repeats, string content)
        {
            var r = "";
            for (var i = 0; i < repeats; i++) r += content;
            return r;
        }

        /// <summary>
        ///     Generate a string with N number of spaces in it
        /// </summary>
        /// <param name="spaces">Number of spaces</param>
        /// <returns>A string containing 'spaces' number of spaces</returns>
        public static string CreateSpacesString(int spaces)
        {
            return CreateRepeatingString(spaces, " ");
        }

        /// <summary>
        ///     Generate a string with N number of tabs in it
        /// </summary>
        /// <param name="tabs">Number of tabs</param>
        /// <returns>A string containing 'tabs' number of tabs</returns>
        public static string CreateTabsString(int tabs)
        {
            return CreateRepeatingString(tabs, "\t");
        }

        /// <summary>
        ///     Generate a string with N number of newlines in it
        /// </summary>
        /// <param name="newLines">Number of newlines</param>
        /// <returns>A string containing 'newLines' number of newlines</returns>
        public static string CreateNewLinesString(int newLines)
        {
            return CreateRepeatingString(newLines, "\n");
        }
    }
}