using System.Text.Json.Serialization;
namespace PolyHaven.API;

public enum PhAssetType
{
	HDRI = 0,
	Texture = 1,
	Model = 2
}

public class PolyHavenAssetInfo
{
	public string Name { get; set; } = "";
	public PhAssetType Type { get; set; }
	[JsonPropertyName( "files_hash" )]
	public string FilesHash { get; set; } = "";
	public Dictionary<string, string> Authors { get; set; } = new();
	public List<string> Categories { get; set; } = new();
	public List<string> Tags { get; set; } = new();
	public Vector2 Dimensions { get; set; } = new();

	public override string ToString()
	{
		return Name;
	}
}
