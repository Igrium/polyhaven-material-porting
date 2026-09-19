using PolyHaven.API;
using PolyHaven.Util;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
namespace PolyHaven.Assets;

public class TextureMaterialAsset : IPolyAsset
{
	/// <summary>
	/// Assets in any of these categories will be sorted into folders of their name.
	/// </summary>
	protected static readonly string[] TopLevelCategories = new string[]
	{
		"arial",
		"terrain",
		"brick",
		"wood",
		"concrete",
		"plaster",
		"fabric",
		"metal",
		"rock",
		"wall",
		"floor",
		"roofing",
		"outdoor"
	};

	public string PolyHavenId { get; init; }

	public PolyHavenAssetInfo Info { get; init; }

	public DownloadedFileList? DownloadedFiles { get; private set; }

	public string? SubfolderName { get; set; }

	public Asset? SBoxAsset { get; private set; }

	public string? AssetPartyUrl { get; set; }

	public TextureMaterialAsset( string polyHavenId, PolyHavenAssetInfo info )
	{
		if ( info.Type != PhAssetType.Texture )
		{
			throw new ArgumentException( "Improper asset type.", nameof( info ) );
		}
		PolyHavenId = polyHavenId;
		Info = info;
		SubfolderName = FindSubfolder( info );
	}

	protected virtual string? FindSubfolder( PolyHavenAssetInfo info )
	{
		foreach ( var cat in TopLevelCategories )
		{
			if ( info.Categories.Contains( cat ) )
				return cat;
		}
		return null;
	}

	/// <summary>
	/// Query PolyHaven for all the textures relating to this material.
	/// </summary>
	/// <returns>The textures.</returns>
	public Task<MaterialTextureList> GetTextures()
	{
		return PolyHavenApi.GetMaterialTextures( PolyHavenId );
	}

	public string LocalBasePath => SubfolderName != null ? "materials/" + SubfolderName : "materials";

	public Task<IEnumerable<string>> DownloadFiles()
	{
		return DownloadFiles( "2k", "1k" );
	}

	/// <summary>
	/// Download all the textures that this material needs.
	/// </summary>
	/// <returns>A task that finishes when the textures have downloaded.</returns>
	public async Task<IEnumerable<string>> DownloadFiles( string resolution = "2k", string aoRes = "2k" )
	{
		if ( DownloadedFiles != null )
		{
			Log.Warning( $"{this} already has its textures downloaded. Replacing..." );
		}
		var activeProject = PortingUtility.RequireProject();

		var localPrefix = $"{LocalBasePath}/tex_{PolyHavenId}";
		var globalPrefix = Path.Combine( activeProject.GetAssetsPath(), localPrefix ).Replace( '\\', '/' );
		MaterialTextureList textures = await GetTextures();

		Log.Info( $"Downloading textures to {globalPrefix}..." );

		List<Task<string>> downloadTasks = new( 6 );
		DownloadedFileList fileList = new();

		FileReference? diff = textures.Diffuse?.GetResolution( resolution )?.JPEG;
		if ( diff.HasValue )
			downloadTasks.Add( CreateDownloadTask( diff.Value, fileList.SetDiffuse, localPrefix, globalPrefix, "color.jpg" ) );

		FileReference? normal = textures.NormalGL?.GetResolution( resolution )?.PNG;
		if ( normal.HasValue )
			downloadTasks.Add( CreateDownloadTask( normal.Value, fileList.SetNormal, localPrefix, globalPrefix, "normal.png" ) );

		// Don't need any higher resolution
		FileReference? height = textures.Displacement?.Res1k?.PNG;
		if ( height.HasValue )
			downloadTasks.Add( CreateDownloadTask( height.Value, fileList.SetHeight, localPrefix, globalPrefix, "height.png" ) );

		FileReference? ao = textures.AO?.GetResolution( aoRes )?.JPEG;
		if ( ao.HasValue )
			downloadTasks.Add( CreateDownloadTask( ao.Value, fileList.SetAO, localPrefix, globalPrefix, "ao.jpg" ) );

		FileReference? rough = textures.Rough?.GetResolution( resolution )?.PNG;
		if ( rough.HasValue )
			downloadTasks.Add( CreateDownloadTask( rough.Value, fileList.SetRough, localPrefix, globalPrefix, "rough.png" ) );

		FileReference? metal = textures.Metal?.GetResolution( resolution )?.JPEG;
		if ( metal.HasValue )
			downloadTasks.Add( CreateDownloadTask( metal.Value, fileList.SetMetal, localPrefix, globalPrefix, "metal.jpg" ) );

		await Task.WhenAll( downloadTasks );
		DownloadedFiles = fileList;
		return fileList;
	}

	private delegate void FileConsumer( string filename );
	private async Task<string> CreateDownloadTask( FileReference file, FileConsumer consumer, string localPrefix, string globalPrefix, string suffix )
	{
		var filename = $"{PolyHavenId}_{suffix}";
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
			throw new InvalidOperationException( "Unable to download from " + file.Url );
		}
	}

	public Asset GenerateMaterial()
	{
		if ( DownloadedFiles == null )
			throw new InvalidOperationException( "Textures have not been downloaded." );

		if ( SBoxAsset != null )
			Log.Warning( "The asset has already been generated. Overriding..." );

		var activeProject = PortingUtility.RequireProject();

		// The textures came down over plain http, so the asset system has never heard of them. Register
		// them before the material that references them, or the vmat compile fails with
		// "Invalid Dependency Information".
		foreach ( var texture in DownloadedFiles )
		{
			AssetSystem.RegisterFile( Path.Combine( activeProject.GetAssetsPath(), texture ) );
		}

		var vmatPath = $"{LocalBasePath}/{PolyHavenId}.vmat";
		Log.Info( "Generating material " + vmatPath );

		Template template;
		if ( DownloadedFiles.Metal != null )
			template = new Template( "simple_metal.template" );
		else
			template = new Template( "simple_standard.template" );

		var vmatContents = template.Parse( new Dictionary<string, string?>
		{
			{ "ColorTexture", DownloadedFiles.Color },
			{ "RoughnessTexture", DownloadedFiles.Rough },
			{ "NormalTexture", DownloadedFiles.Normal },
			{ "AOTexture", DownloadedFiles.AO },
			{ "MetalnessTexture", DownloadedFiles.Metal }
		} );

		var globalPath = Path.Combine( activeProject.GetAssetsPath(), vmatPath );
		Directory.CreateDirectory( Path.GetDirectoryName( globalPath )! );
		File.WriteAllText( globalPath, vmatContents );

		// RegisterFile asserts it's on the main thread, and hands the asset straight back now -
		// no need for a separate FindByPath.
		SBoxAsset = AssetSystem.RegisterFile( globalPath ) ?? AssetSystem.FindByPath( vmatPath );
		if ( SBoxAsset == null )
			throw new InvalidOperationException( $"Wrote {vmatPath} but the asset system didn't pick it up." );

		Log.Info( $"Wrote material to {SBoxAsset}" );
		return SBoxAsset;
	}

	public void SetupMetadata()
	{
		if ( SBoxAsset == null )
			throw new InvalidOperationException( "Material has not been generated." );

		SBoxAsset.MetaData.Set( "polyhaven_id", PolyHavenId );
		SBoxAsset.Publishing.CreateTemporaryProject();

		var tags = new HashSet<string>();
		foreach ( var tag in Info.Tags )
			tags.Add( tag );
		foreach ( var tag in Info.Categories )
			tags.Add( tag );

		// ProjectConfig no longer carries Tags - modern s&box sets those on the package page - so we
		// stash the scrape on the asset instead of losing it.
		SBoxAsset.MetaData.Set( "polyhaven_tags", PortingUtility.ReplaceSpaces( tags ).ToArray() );

		// The engine validates and truncates idents itself now, so we can just hand it the PolyHaven id.
		SBoxAsset.Publishing.ProjectConfig.Ident = PolyHavenId;
		SBoxAsset.Publishing.ProjectConfig.Title = Info.Name;
		SBoxAsset.Publishing.ProjectConfig.Org = "polyhaven";
		SBoxAsset.Publishing.Save();

		SBoxAsset.MetaData.Set( "PolyAsset", Info );
	}

	public override string ToString()
	{
		return $"TextureMaterialAsset[{PolyHavenId}]";
	}

	public class DownloadedFileList : IEnumerable<string>
	{
		public string? Color;
		public string? Normal;
		public string? Height;
		public string? AO;
		public string? Rough;
		public string? Metal;

		public void SetDiffuse( string val ) { Color = val; }
		public void SetNormal( string val ) { Normal = val; }
		public void SetHeight( string val ) { Height = val; }
		public void SetAO( string val ) { AO = val; }
		public void SetRough( string val ) { Rough = val; }
		public void SetMetal( string val ) { Metal = val; }

		public IEnumerator<string> GetEnumerator()
		{
			if ( Color != null ) yield return Color;
			if ( Normal != null ) yield return Normal;
			if ( Height != null ) yield return Height;
			if ( AO != null ) yield return AO;
			if ( Rough != null ) yield return Rough;
			if ( Metal != null ) yield return Metal;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
