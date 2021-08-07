using System;
using System.Linq;
using Serilog;

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
            return emoteName.Trim().Replace(" ", "_").TrimTo(32, true);
        }
        
        public static string TrimTo(this string str, int maxLength, bool showDots = false)
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

            if (showDots == false)
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
        
        public static bool IsSmallEnough(this byte[] fileSize, out string size)
        {
            if (fileSize.Length > 256000)
            {
                size = fileSize.LongLength.FormatSize();
                return false;
            }

            size = fileSize.LongLength.FormatSize();
            return true;
        }
        
        public static string FormatSize(this long bytes)
        {
            var suffixes = new[]
            {
                "Bytes",
                "KB",
                "MB",
                "GB",
                "TB",
                "PB"
            };
            
            var counter = 0;
            var number = (decimal) bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1}{suffixes[counter]}";
        }
    }
}