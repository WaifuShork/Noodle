using System;
using System.Linq;
using Noodle.Utilities;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Noodle.Common.Models
{
    public class Root
    {
        [JsonPropertyName("emotes")]
        public List<EmoteModel> Emotes { get; set; }    
    }
    
    public class EmoteModel
    {
        public string Name { get; set; }
        public string Base64 { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public bool IsAnimated { get; set; }
    }

    public static class DatabaseUtilities
    {
        private static readonly JsonSerializerOptions _options;

        static DatabaseUtilities()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }
            
        public static async Task<Root> LoadAsync(string path)
        {
            return await JsonUtilities.ReadFromFile<Root>(path, _options);
        }
        
        public static async Task SaveAsync(Root root, string path)
        {
            await JsonUtilities.WriteToFile(root, path, _options);
        }

        public static async Task AddAsync(EmoteModel emote, string path)
        {
            var root = await LoadAsync(path);
            root.Emotes.Add(emote);
            await SaveAsync(root, path);
        }

        public static async Task<EmoteModel> GetAsync(string name, string path, string category = "null")
        {
            var root = await LoadAsync(path);
            var emote = root.Emotes.FirstOrDefault(e => e.Name == name && e.Category == category);
            if (emote == null)
            {
                throw new NullReferenceException($"Unable to locate **{name}** in the category **{category}**");
            }

            return emote;
        }

        public static async Task<IEnumerable<EmoteModel>> GetAllAsync(string path)
        {
            var root = await LoadAsync(path);
            return root.Emotes ?? new List<EmoteModel>();
        }

        public static async Task RemoveAsync(string name, string category, string path)
        {
            var root = await LoadAsync(path);
            var emote = await GetAsync(name, path, category);
            root.Emotes.RemoveAll(e => e.Name == emote.Name);
            await SaveAsync(root, path);
        }
    }
}