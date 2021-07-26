using System;
using System.Threading.Tasks;
using Discord.Commands;
using ImageMagick;

namespace Noodle.TypeReaders
{
    public class FontStretchTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            FontStretch result;
            switch (input.ToLowerInvariant())
            {
                case "undefined":
                    result = FontStretch.Undefined;
                    break;
                case "normal":
                    result = FontStretch.Normal;
                    break;
                case "ultracondensed":
                    result = FontStretch.UltraCondensed;
                    break;
                case "extracondensed":
                    result = FontStretch.ExtraCondensed;
                    break;
                case "condensed":
                    result = FontStretch.Condensed;
                    break;
                case "semicondensed":
                    result = FontStretch.SemiCondensed;
                    break;
                case "semiexpanded":
                    result = FontStretch.SemiExpanded;
                    break;
                case "expanded":
                    result = FontStretch.Expanded;
                    break;
                case "extraexpanded":
                    result = FontStretch.ExtraExpanded;
                    break;
                case "ultraexpanded":
                    result = FontStretch.UltraExpanded;
                    break;
                case "any":
                    result = FontStretch.Any;
                    break;
                default:
                    return Task.FromResult(TypeReaderResult.FromError(new Exception($"Unable to parse {input}")));

            }
            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}