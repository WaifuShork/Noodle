using System.Threading.Tasks;

namespace Noodle 
{
    internal static class Program 
    {
        private static async Task<int> Main()
        {
            return await NoodleHost.RunAsync();
        }
    }
}
