using System.Net.Http;

namespace Noodle
{
    public static class Constants
    {
        static Constants()
        {
            HttpClient = new HttpClient();
        }
        
        public static HttpClient HttpClient { get; }
    }
}