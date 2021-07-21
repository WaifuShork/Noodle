using System;
using System.Linq;

namespace Noodle.Extensions
{
    public static class StringExtensions
    {
        public static string SanitizeUrl(this string url)
        {
            if (url.Contains("<") && url.Contains(">"))
            {
                return url
                    .Replace("<", "")
                    .Replace(">", "");
            }

            return url;
        }
        
        public static string SanitizeEmoteName(this string emoteName)
        {
            return emoteName.Trim().Replace(" ", "_").TrimTo(32);
        }
        
        public static string TrimTo(this string str, int maxLength, bool hideDots = false)
        {
            if (maxLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength), $"Argument {nameof(maxLength)} can't be negative.");
            }
            if (maxLength == 0)
            {
                return string.Empty;
            }
            if (maxLength <= 3)
            {
                return string.Concat(str.Select(_ => '.'));
            }
            if (str.Length < maxLength)
            {
                return str;
            }

            if (hideDots)
            {
                return string.Concat(str.Take(maxLength));
            }
            
            return string.Concat(str.Take(maxLength - 3)) + "...";
        }

        public static string AsNullIfEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }

        public static string WithAlternative(this string str, string alternative)
        {
            return str.AsNullIfEmpty() ?? alternative;
        }

        public static string SanitizeModule(this string moduleName)
        {
            if (moduleName.Contains("module"))
            {
                return moduleName.Replace("module", "");
            }

            if (moduleName.Contains("Module"))
            {
                return moduleName.Replace("Module", "");
            }

            return moduleName;
        }
    }
}