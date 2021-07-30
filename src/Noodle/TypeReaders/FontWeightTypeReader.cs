using System;
using System.Threading.Tasks;
using Discord.Commands;
using ImageMagick;

namespace Noodle.TypeReaders
{
    public class FontWeightTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
            IServiceProvider services)
        {
            FontWeight result;
            switch (input.ToLowerInvariant())
            {
                case "undefined":
                    result = FontWeight.Undefined;
                    break;
                case "thin":
                    result = FontWeight.Thin;
                    break;
                case "extralight":
                    result = FontWeight.ExtraLight;
                    break;
                case "light":
                    result = FontWeight.Light;
                    break;
                case "normal":
                    result = FontWeight.Normal;
                    break;
                case "medium":
                    result = FontWeight.Medium;
                    break;
                case "demibold":
                    result = FontWeight.DemiBold;
                    break;
                case "bold":
                    result = FontWeight.Bold;
                    break;
                case "extrabold":
                    result = FontWeight.ExtraBold;
                    break;
                case "ultrabold":
                    result = FontWeight.UltraBold;
                    break;
                case "heavy":
                    result = FontWeight.Heavy;
                    break;
                default:
                    return Task.FromResult(TypeReaderResult.FromError(new Exception($"Unable to parse {input}")));

            }
            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}