using System;
using System.Linq;
using System.Text;

namespace Noodle.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder TAppend<T>(this StringBuilder builder, T item)
        {
            return builder.Append(item);
        }

        public static StringBuilder TAppendLine<T>(this StringBuilder builder, T item, int newLines = 0)
        {
            builder.AppendLine(item.ToString());
            if (newLines > 0)
            {
                for (var i = 0; i < newLines; i++)
                {
                    builder.AppendLine();
                }
            }

            return builder;
        }

        public static StringBuilder TAppendLines<T>(this StringBuilder builder, params T[] items)
        {
            foreach (var item in items)
            {
                builder.TAppendLine(item);
            }

            return builder;
        }
    }
}