using System;
using System.Text;

namespace NeeLaboratory.Text
{
    // from https://stackoverflow.com/questions/40347168/how-to-parse-an-escape-sequence
    public static class StringExtensions
    {
        private enum UnescapeState
        {
            Unescaped,
            Escaped
        }

        public static string Unescape(this string s)
        {
            var sb = new StringBuilder(s.Length + 2);
            var state = UnescapeState.Unescaped;

            foreach (var ch in s)
            {
                switch (state)
                {
                    case UnescapeState.Escaped:
                        switch (ch)
                        {
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;

                            case '\\':
                            case '\"':
                                sb.Append(ch);
                                break;

                            default:
                                //throw new Exception("Unrecognized escape sequence '\\" + ch + "'");
                                sb.Append('\\');
                                sb.Append(ch);
                                break;
                        }
                        state = UnescapeState.Unescaped;
                        break;

                    case UnescapeState.Unescaped:
                        if (ch == '\\')
                        {
                            state = UnescapeState.Escaped;
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            if (state == UnescapeState.Escaped)
            {
                //throw new Exception("Unterminated escape sequence");
                sb.Append('\\');
            }

            return sb.ToString();
        }
    }
}
