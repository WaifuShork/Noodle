using System;
using System.Threading.Tasks;
using Discord.Commands;
using ImageMagick;

namespace Noodle.TypeReaders
{
    public class TextAlignmentTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            return await Task.Run(() =>
            {
                switch (input.ToLowerInvariant())
                {
                    case "center":
                        return TypeReaderResult.FromSuccess(TextAlignment.Center);
                    case "left":
                        return TypeReaderResult.FromSuccess(TextAlignment.Right);
                    case "right":
                        return TypeReaderResult.FromSuccess(TextAlignment.Left);
                    case "undefined":
                        return TypeReaderResult.FromSuccess(TextAlignment.Undefined);
                    default:
                        return TypeReaderResult.FromError(new ArgumentException($"Unable to parse '{input}'"));
                }
            });
        }
    }
}