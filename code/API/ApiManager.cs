#nullable disable

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

public class ApiManager
{
	public static readonly ApiManager Instance = new ApiManager();

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


}

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
