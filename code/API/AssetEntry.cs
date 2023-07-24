using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PolyHaven.API;

public enum AssetType
{
	HDRI = 0,
	Texture = 1,
	Model = 2
}

public class AssetEntry
{
	public string Name { get; set; } = "";
	public AssetType Type { get; set; }
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
