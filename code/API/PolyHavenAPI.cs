#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static PolyHaven.API.DeserializationBullshit;

namespace PolyHaven.API;

public class PolyHavenAPI
{
	public static readonly PolyHavenAPI Instance = new PolyHavenAPI();

	public HttpClient Client { get; set; } = new HttpClient()
	{
		BaseAddress = new Uri( "https://api.polyhaven.com" )
	};

	/// <summary>
	/// Get all the assets from PolyHaven
	/// </summary>
	/// <param name="category">Category to look in</param>
	/// <returns>Dictionary with all the assets.</returns>
	public async Task<Dictionary<string, AssetEntry>> GetAssets(string? category = null)
	{
		var url = category == null ? "assets" : "assets?t=" + category;
		var dict = await Client.GetFromJsonAsync<Dictionary<string, AssetEntry>>( url );

		return dict != null ? dict : new();
	}

	public async Task<AssetEntry?> GetAsset(string assetID)
	{
		var url = "info/" + assetID;
		return await Client.GetFromJsonAsync<AssetEntry>( url );
	}

	/// <summary>
	/// Get a dictionary of all the resolutions of a given hdr id.
	/// </summary>
	/// <param name="id">HDR id</param>
	/// <returns>A dictionary with the resolution name and the file entry.</returns>
	/// <exception cref="InvalidOperationException">If the server returns unexpected responses.</exception>
	public async Task<Dictionary<string, FileReference>> GetHDRFiles(string id)
	{
		var url = $"files/{id}";
		var response = await Client.GetAsync( url );
		response.EnsureSuccessStatusCode();

		// geezus
		var json = await response.Content.ReadFromJsonAsync<FileResRoot>();
		if (json == null)
		{
			throw new InvalidOperationException( "Poly haven returned no response." );
		}

		Dictionary<string, FileReference> dict = new();

		foreach (var res in json.hdri)
		{
			dict.Add( res.Key, res.Value.exr );
		}

		return dict;
	}

	public async Task<MaterialTextureList> GetMaterialTextures(string id)
	{
		var url = $"files/{id}";
		var response = await Client.GetAsync( url );
		response.EnsureSuccessStatusCode();

		var texList = await response.Content.ReadFromJsonAsync<MaterialTextureList>();
		return texList;
	}
}

public struct MaterialTextureList
{
	public struct TextureEntry
	{
		public struct TextureResolution
		{
			[JsonPropertyName("jpg")]
			public FileReference JPEG { get; set; }
			[JsonPropertyName("png")]
			public FileReference PNG { get; set; }
			[JsonPropertyName("exr")]
			public FileReference EXR { get; set; }
		}

		[JsonPropertyName("8k")]
		public TextureResolution? Res8k { get; set; }
		[JsonPropertyName("4k")]
		public TextureResolution? Res4k { get; set; }
		[JsonPropertyName("2k")]
		public TextureResolution? Res2k { get; set; }
		[JsonPropertyName("1k")]
		public TextureResolution? Res1k { get; set; }
	}


	public TextureEntry? Diffuse { get; set; }
	[JsonPropertyName("nor_dx")]
	public TextureEntry? NormalDX { get; set; }
	[JsonPropertyName("nor_gl")]
	public TextureEntry? NormalGL { get; set; }
	public TextureEntry? Displacement { get; set; }
	public TextureEntry? AO { get; set; }
	public TextureEntry? Rough { get; set; }

}

#nullable disable
internal static class DeserializationBullshit
{
	public class Resolution
	{
		public FileReference hdr { get; set; }
		public FileReference exr { get; set; }
	}

	public class FileResRoot
	{
		public Tonemapped tonemapped { get; set; }
		public Dictionary<string, Resolution> hdri { get; set; }
	}

	public class Tonemapped
	{
		public int size { get; set; }
		public string md5 { get; set; }
		public string url { get; set; }
	}
}
