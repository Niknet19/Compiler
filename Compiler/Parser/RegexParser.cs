using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Compiler.Parser
{


    public class RegexParser
    {
        private static readonly Regex RussianPhoneRegex = new Regex(@"(?:\d[ -]?){6}\d", RegexOptions.Compiled);
        private static readonly Regex Isbn13Regex = new Regex(@"(?:ISBN[- ]?13)?[- ]?(?=\d{13}$)\d{1,5}([- ]?)\d{1,7}\1\d{1,6}\1\d", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
        public static string[] FindAndFormatMatches(string input)
        {
            var results = new List<string>();

            // Проверяем все три regex на входной строке
            results.AddRange(FormatMatches(input, RussianPhoneRegex, "Номер"));
            results.AddRange(FormatMatches(input, Isbn13Regex, "ISBN-13"));
            results.AddRange(FormatMatches(input, EmailRegex, "Email"));

            return results.ToArray();
        }

        private static IEnumerable<string> FormatMatches(string input, Regex regex, string patternType)
        {
            var matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                yield return $"[{patternType}] Найдено: '{match.Value}' на позиции {match.Index}\n";
            }
        }
    }

}
