using Editor;
using PolyHaven.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Assets;

public class TextureMaterialAsset : IPolyAsset
{
	public string PolyHavenID { get; init; }

	public AssetEntry Asset { get; init; }

	public DownloadedFileList? DownloadedFiles { get; private set; }

	public Editor.Asset? SBoxAsset { get; private set; }

	public TextureMaterialAsset( string polyHavenID, AssetEntry asset )
	{
		if ( asset.Type != API.AssetType.Texture )
		{
			throw new ArgumentException( "Improper asset type.", nameof( asset ) );
		}
		PolyHavenID = polyHavenID;
		Asset = asset;
	}

	/// <summary>
	/// Create a poly asset from a polyhaven id, scraping the metadata from PolyHaven.
	/// </summary>
	/// <param name="id">The ID</param>
	/// <returns>The entry</returns>
	/// <exception cref="ArgumentException">If the ID cannot be found.</exception>
	public static async Task<TextureMaterialAsset> Get( string id )
	{
		AssetEntry? entry = await PolyHavenAPI.Instance.GetAsset( id );
		if ( entry == null )
		{
			throw new ArgumentException( "Cannot find texture with that ID.", nameof( id ) );
		}

		return new TextureMaterialAsset( id, entry );
	}

	/// <summary>
	/// Query PolyHaven for all the textures relating to this material.
	/// </summary>
	/// <returns>The textures.</returns>
	public Task<MaterialTextureList> GetTextures()
	{
		return PolyHavenAPI.Instance.GetMaterialTextures( PolyHavenID );
	}

	/// <summary>
	/// Download all the textures that this material needs.
	/// </summary>
	/// <returns>A task that finishes when the textures have downloaded.</returns>
	/// <exception cref="InvalidOperationException">If there is no active project.</exception>
	public async Task<IEnumerable<string>> DownloadFiles()
	{
		if ( DownloadedFiles != null )
		{
			Log.Warning( $"{this} already has its textures downloaded. Replacing..." );
		}
		var activeProject = PolySettings.Instance.ActiveProject;
		if ( activeProject == null )
		{
			throw new InvalidOperationException( "No active project." );
		}
		var localPrefix = $"materials/tex_{PolyHavenID}";
		var globalPrefix = Path.Combine( activeProject.GetAssetsPath(), localPrefix ).Replace( '\\', '/' );
		MaterialTextureList textures = await GetTextures();

		Log.Info( $"Downloading textures to {globalPrefix}..." );

		List<Task<string>> DownloadTasks = new( 5 );
		DownloadedFileList fileList = new();

		FileReference? diff = textures.Diffuse?.Res2k?.JPEG;
		if ( diff.HasValue )
			DownloadTasks.Add( CreateDownloadTask( diff.Value, fileList.SetDiffuse, localPrefix, globalPrefix, "color.jpg" ) );

		FileReference? normal = textures.NormalGL?.Res2k?.PNG;
		if ( normal.HasValue )
			DownloadTasks.Add( CreateDownloadTask( normal.Value, fileList.SetNormal, localPrefix, globalPrefix, "normal.png" ) );

		FileReference? height = textures.Displacement?.Res2k?.PNG;
		if ( height.HasValue )
			DownloadTasks.Add( CreateDownloadTask( height.Value, fileList.SetHeight, localPrefix, globalPrefix, "height.png" ) );

		FileReference? ao = textures.AO?.Res2k?.JPEG;
		if ( ao.HasValue )
			DownloadTasks.Add( CreateDownloadTask( ao.Value, fileList.SetAO, localPrefix, globalPrefix, "ao.jpg" ) );

		FileReference? rough = textures.Rough?.Res2k?.JPEG;
		if ( rough.HasValue )
			DownloadTasks.Add( CreateDownloadTask( rough.Value, fileList.SetRough, localPrefix, globalPrefix, "rough.jpg" ) );

		await Task.WhenAll( DownloadTasks );
		DownloadedFiles = fileList;
		return fileList;
	}

	public void GenerateSBoxAsset( bool useDisplacement = false )
	{
		if (DownloadedFiles == null)
		{
			throw new InvalidOperationException( "Textures have not been downloaded." );
		}
	}

	private delegate void FileConsumer( string filename );
	private async Task<string> CreateDownloadTask( FileReference file, FileConsumer consumer, string localPrefix, string globalPrefix, string suffix )
	{
		var filename = $"{PolyHavenID}_{suffix}";
		bool success = await file.DownloadAsync( globalPrefix + "/" + filename );
		if ( success )
		{
			string output = localPrefix + "/" + filename;
			Log.Info( "Saved texture to " + output );
			consumer.Invoke( output );
			return output;
		}
		else
		{
			throw new InvalidOperationException( "Unable to download from " + file.URL );
		}
	}

	public override string ToString()
	{
		return $"TextureMaterialAsset[{PolyHavenID}]";
	}

	public class DownloadedFileList : IEnumerable<string>
	{
		public string? Diffuse;
		public string? Normal;
		public string? Height;
		public string? AO;
		public string? Rough;

		public void SetDiffuse( string val ) { Diffuse = val; }
		public void SetNormal( string val ) { Normal = val; }
		public void SetHeight( string val ) { Height = val; }
		public void SetAO( string val ) { AO = val; }
		public void SetRough( string val ) { Rough = val; }


		public IEnumerator<string> GetEnumerator()
		{
			if ( Diffuse != null ) yield return Diffuse;
			if ( Normal != null ) yield return Normal;
			if ( Height != null ) yield return Height;
			if ( AO != null ) yield return AO;
			if ( Rough != null ) yield return Rough;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
