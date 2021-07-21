using System.Data;
using System.Threading.Tasks;
    
using Discord.Commands;

using Noodle.Attributes;

namespace Noodle.Modules
{
    [ModuleName("Example")]
    public class ExampleModule : NoodleModuleBase
    {
        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
        }
        
        [Command("math")]
        public async Task MathAsync([Remainder] string math)
        {
            var dataTable = new DataTable();
            var result = dataTable.Compute(math, null);
            
            await ReplyAsync($"Result: {result}");
        }
    }
}