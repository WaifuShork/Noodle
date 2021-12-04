using System.Text.Json.Serialization;

namespace Noodle;

public class Configuration
{
	[JsonPropertyName("token")]
	public string Token { get; set; }
	
	[JsonPropertyName("prefix")]
	public string Prefix { get; set; }
}