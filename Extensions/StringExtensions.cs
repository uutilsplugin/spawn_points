using System.Text.RegularExpressions;

namespace UUtils.Utilities
{
    public static class StringExtensions
    {
        public static string ConvertWhitespacesToSingleSpaces(this string _input)
        {
            return Regex.Replace(_input, @"\s+", " ");
        }

        /// <summary>
        /// Splits a string on tabs ('\u0009')
        /// </summary>
        /// <returns>The tabs.</returns>
        /// <param name="_input">Input.</param>
        public static string[] SplitTabs(this string _input)
        {
            char _tab = '\u0009';
            return _input.Split(_tab);
        }

        /// <summary>
        /// Trims a string start and end of tabs ('\u0009')
        /// </summary>
        public static string TrimWhiteSpaceTabs(this string _input)
        {
            char _tab = '\u0009';
            return _input.TrimStart(_tab).TrimEnd(_tab);
        }

        public static string RemoveAllSpecialCharacters(this string _input)
        {
            return Regex.Replace(_input, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        /// <summary>
        /// Replaces all tab characters ('\u0009')
        /// </summary>
        public static string ReplaceTabs(this string _input, string _replacement)
        {
            char _tab = '\u0009';
            return _input.Replace(_tab.ToString(), _replacement);
        }

        /// <summary>
        /// Removes all text after index and optionally ads "..."
        /// </summary>
        /// <returns>The short text.</returns>
        /// <param name="_input">Input.</param>
        /// <param name="_startIndex">Start removing from this index.</param>
        /// <param name="_addEtc">Add "..." at the end</param>
        public static string ShorterText(this string _input, int _startIndex, bool _addEtc)
        {
            string _output = _input;

            if (_input.Length > 0)
            {
                if (_startIndex < 0)
                {
                    _startIndex = 0;
                }

                if (_input.Length > _startIndex)
                {
                    _output = _input.Remove(_startIndex - 1, _input.Length - _startIndex + 1);
                    if (_addEtc)
                    {
                        _output += "...";
                    }
                }
            }

            return _output;
        }
    }
}