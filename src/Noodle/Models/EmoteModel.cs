using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Noodle.Models
{
    public class Root
    {
        [JsonPropertyName("emotes")]
        public List<EmoteModel> Emotes { get; set; }    
    }
    
    public class EmoteModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("base64")]
        public string Base64 { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; } // as a backup
        [JsonPropertyName("category")]
        public string Category { get; set; }
    }

    public static class DatabaseUtilities
    {
        public static async Task<Root> LoadAsync(string path)
        {
            var content = await File.ReadAllTextAsync(path);
            var bytes = Encoding.UTF8.GetBytes(content);
            using var stream = new MemoryStream(bytes);
            return await JsonSerializer.DeserializeAsync<Root>(stream);
        }
        
        public static async Task SaveAsync(Root root, string path)
        {
            var options = new JsonSerializerOptions {WriteIndented = true};
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, root, options);
            await using var fs = File.Create(path);
            await stream.CopyToAsync(fs);
        }

        public static async Task AddAsync(EmoteModel emote, string path)
        {
            var root = await LoadAsync(path);
            root.Emotes.Add(emote);
            await SaveAsync(root, path);
        }

        public static async Task<EmoteModel> GetAsync(string name, string path)
        {
            var root = await LoadAsync(path);
            return root.Emotes.FirstOrDefault(e => e.Name == name);
        }

        public static async Task RemoveAsync(string name, string category, string path)
        {
            var root = await LoadAsync(path);
            var emote = root.Emotes.FirstOrDefault(e => e.Name == name && e.Category == category);
            if (emote == null)
            {
                throw new ArgumentNullException(nameof(emote), $"Unable to locate {name} in the category {category}");
            }
            
            root.Emotes.Remove(emote);
            await SaveAsync(root, path);
        }
    }
}