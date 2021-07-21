using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Noodle.TypeReaders
{
    public class EmoteTypeReader : TypeReader
    {
        private readonly Regex _emotePattern = new(@"^<a?:.+:\d+>$");
        
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            IEmote result;

            if (_emotePattern.IsMatch(input))
            {
                result = Emote.Parse(input);
            }
            else if (input.Length == 1)
            {
                result = new Emoji(input);
            }
            else
            {
                throw new ArgumentException($"Value {input} is not a valid emote");
            }

            return await Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}