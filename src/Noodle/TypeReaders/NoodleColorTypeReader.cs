using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Noodle.TypeReaders
{
    public class NoodleColorTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            throw new NotImplementedException();
        }
    }
}