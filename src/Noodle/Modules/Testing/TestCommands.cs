using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Noodle.Modules
{
    public sealed partial class TestingModule
    {
        [Command("throw")]
        public async Task ThrowAsync()
        {
            throw new Exception("AAAAAAAAAAAAAAAAAHH");
        }
    }
}