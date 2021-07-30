using System;
using System.Threading.Tasks;
using Discord.Commands;
using ImageMagick;

namespace Noodle.TypeReaders
{
    public class FontStyleTypeTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
            IServiceProvider services)
        {
            FontStyleType result;
            switch (input.ToLowerInvariant())
            {
                case "undefined":
                    result = FontStyleType.Undefined;
                    break;
                case "normal":
                    result = FontStyleType.Normal;
                    break;
                case "italic":
                    result = FontStyleType.Italic;
                    break;
                case "oblique":
                    result = FontStyleType.Oblique;
                    break;
                case "any":
                    result = FontStyleType.Any;
                    break;
                case "bold":
                    result = FontStyleType.Bold;
                    break;
                default:
                    return Task.FromResult(TypeReaderResult.FromError(new Exception($"Unable to parse {input}")));

            }
            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}