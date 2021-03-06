using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Noodle.Utilities
{
    public static class JsonUtilities
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        public static async Task WriteToFile<T>(T item, string path, JsonSerializerOptions options = null) where T : notnull
        {
            options ??= JsonSerializerOptions;

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "unable to locate path");
            }
            
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "expected non-null value");
            }
            
            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, item, options);
            await File.WriteAllTextAsync(path, Utilities.UTF8.GetString(stream.ToArray()));
        }

        public static async Task<T> ReadFromFile<T>(string path, JsonSerializerOptions options = null)
        {
            options ??= JsonSerializerOptions;
            
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "unable to locate path");
            }
            
            var contents = await File.ReadAllTextAsync(path);
            await using var stream = new MemoryStream(Utilities.UTF8.GetBytes(contents));
            return await JsonSerializer.DeserializeAsync<T>(stream, options);
        }
    }
}