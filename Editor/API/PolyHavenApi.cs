using PolyHaven.Assets;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace PolyHaven.API;

public static class PolyHavenApi
{
	public static HttpClient Client { get; } = new HttpClient()
	{
		BaseAddress = new Uri( "https://api.polyhaven.com" )
	};

	/// <summary>
	/// Get all the assets from PolyHaven
	/// </summary>
	/// <param name="category">Category to look in</param>
	/// <returns>Dictionary with all the assets.</returns>
	public static async Task<Dictionary<string, PolyHavenAssetInfo>> GetAssets( string? category = null )
	{
		var url = category == null ? "assets" : "assets?t=" + category;
		var dict = await Client.GetFromJsonAsync<Dictionary<string, PolyHavenAssetInfo>>( url );

		return dict ?? new();
	}

	public static async Task<PolyHavenAssetInfo?> GetAsset( string assetId )
	{
		var url = "info/" + assetId;
		return await Client.GetFromJsonAsync<PolyHavenAssetInfo>( url );
	}

	/// <summary>
	/// Get a dictionary of all the resolutions of a given hdr id.
	/// </summary>
	/// <param name="id">HDR id</param>
	/// <returns>A dictionary with the resolution name and the file entry.</returns>
	/// <exception cref="InvalidOperationException">If the server returns unexpected responses.</exception>
	public static async Task<Dictionary<string, FileReference>> GetHdrFiles( string id )
	{
		var url = $"files/{id}";
		var response = await Client.GetAsync( url );
		response.EnsureSuccessStatusCode();

		// geezus
		var json = await response.Content.ReadFromJsonAsync<DeserializationBullshit.FileResRoot>();
		if ( json?.hdri == null )
		{
			throw new InvalidOperationException( "Poly haven returned no response." );
		}

		return json.hdri.ToDictionary( res => res.Key, res => res.Value.exr );
	}

	public static async Task<MaterialTextureList> GetMaterialTextures( string id )
	{
		var url = $"files/{id}";
		var response = await Client.GetAsync( url );
		response.EnsureSuccessStatusCode();

		var texList = await response.Content.ReadFromJsonAsync<MaterialTextureList?>();
		if ( texList == null )
		{
			throw new InvalidOperationException( "Poly haven returned no response." );
		}

		return texList.Value;
	}

	public static async Task<HdriAsset> GetHdriAsset( string id )
	{
		var info = await GetAsset( id );
		if ( info == null || info.Type != PhAssetType.HDRI )
			throw new ArgumentException( "The supplied asset must be an HDRI.", nameof( id ) );

		return new HdriAsset( id, info );
	}

	public static async Task<TextureMaterialAsset> GetTextureAsset( string id )
	{
		var info = await GetAsset( id );
		if ( info == null || info.Type != PhAssetType.Texture )
			throw new ArgumentException( "The supplied asset must be a texture.", nameof( id ) );

		return new TextureMaterialAsset( id, info );
	}
}

public record struct FileReference
{
	[JsonPropertyName( "url" )]
	public required string Url { get; init; }
	[JsonPropertyName( "md5" )]
	public required string MD5 { get; init; }
	[JsonPropertyName( "size" )]
	public required ulong Size { get; init; }

	public Task<bool> DownloadAsync( string filepath )
	{
		Directory.CreateDirectory( Path.GetDirectoryName( filepath )! );
		return EditorUtility.DownloadAsync( Url, filepath );
	}

	public override string ToString() => $"FileReference[Size={Size}, Hash={MD5}, URL={Url}]";
}


public struct MaterialTextureList
{
	public struct TextureEntry
	{
		public struct TextureResolution
		{
			[JsonPropertyName( "jpg" )]
			public FileReference JPEG { get; set; }
			[JsonPropertyName( "png" )]
			public FileReference PNG { get; set; }
			[JsonPropertyName( "exr" )]
			public FileReference EXR { get; set; }
		}

		[JsonPropertyName( "8k" )]
		public TextureResolution? Res8k { get; set; }
		[JsonPropertyName( "4k" )]
		public TextureResolution? Res4k { get; set; }
		[JsonPropertyName( "2k" )]
		public TextureResolution? Res2k { get; set; }
		[JsonPropertyName( "1k" )]
		public TextureResolution? Res1k { get; set; }

		public TextureResolution? GetResolution( string res )
		{
			if ( res == "8k" ) return Res8k;
			if ( res == "4k" ) return Res4k;
			if ( res == "2k" ) return Res2k;
			if ( res == "1k" ) return Res1k;
			return null;
		}
	}


	public TextureEntry? Diffuse { get; set; }
	[JsonPropertyName( "nor_dx" )]
	public TextureEntry? NormalDX { get; set; }
	[JsonPropertyName( "nor_gl" )]
	public TextureEntry? NormalGL { get; set; }
	public TextureEntry? Displacement { get; set; }
	public TextureEntry? AO { get; set; }
	public TextureEntry? Rough { get; set; }
	public TextureEntry? Metal { get; set; }
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
