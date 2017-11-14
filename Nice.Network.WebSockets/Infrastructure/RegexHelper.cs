using System.Text.RegularExpressions;

namespace Nice.Network.WebSockets
{
    public class RegexHelper
    {
        public static MatchCollection GetRegexValue(string input, string pattern)
        {
            MatchCollection matches = Regex.Matches(input, pattern);
            return matches;
        }
    }
}
