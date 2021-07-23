using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Noodle.TypeReaders
{
    public class FileExtensionTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            switch (input.ToLowerInvariant())
            {
                case "gif":
                    return TypeReaderResult.FromSuccess(EmoteType.Gif);
                case "png":
                    return TypeReaderResult.FromSuccess(EmoteType.Png);
                case "hack":
                    return TypeReaderResult.FromSuccess(EmoteType.Hack);
                default:
                    return TypeReaderResult.FromError(new ArgumentException("Input must be gif or png"));
                
            }
        }
    }

    public enum EmoteType
    {
        Gif,
        Png,
        Hack
    }
}