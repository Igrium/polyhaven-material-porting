using Editor;
using PolyHaven.API;
using PolyHaven.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
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

	protected struct CatEntry
	{
		public string Name;
		public string FolderName;

		public CatEntry( string name )
		{
			Name = name;
			FolderName = name;
		}

		public CatEntry( string name, string folderName )
		{
			Name = name;
			FolderName = folderName;
		}

		public static implicit operator CatEntry( string name ) => new CatEntry( name );
	}

	//protected static readonly Dictionary<string, string> TopLevelCategories = new()
	//{
	//	{ "arial", "arial" },
	//	{ "terrain", "terrain" },
	//	{ "floor", "floor" },

	//};

	public string PolyHavenID { get; init; }

	public AssetEntry Asset { get; init; }

	public DownloadedFileList? DownloadedFiles { get; private set; }

	public string? SubfolderName { get; set; }

	public Editor.Asset? SBoxAsset { get; private set; }

	public TextureMaterialAsset( string polyHavenID, AssetEntry asset )
	{
		if ( asset.Type != API.AssetType.Texture )
		{
			throw new ArgumentException( "Improper asset type.", nameof( asset ) );
		}
		PolyHavenID = polyHavenID;
		Asset = asset;
		SubfolderName = FindSubfolder( asset );
	}

	protected virtual string? FindSubfolder( AssetEntry asset )
	{
		foreach ( var cat in TopLevelCategories )
		{
			if ( asset.Categories.Contains( cat ) )
				return cat;
		}
		return null;
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

	public string LocalBasePath => SubfolderName != null ? "materials/" + SubfolderName : "materials";

	public Task<IEnumerable<string>> DownloadFiles()
	{
		return DownloadFiles( "2k", "1k" );
	}

	/// <summary>
	/// Download all the textures that this material needs.
	/// </summary>
	/// <returns>A task that finishes when the textures have downloaded.</returns>
	/// <exception cref="InvalidOperationException">If there is no active project.</exception>
	public async Task<IEnumerable<string>> DownloadFiles(string resolution = "2k", string aoRes = "1k")
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
		var localPrefix = $"{LocalBasePath}/tex_{PolyHavenID}";
		var globalPrefix = Path.Combine( activeProject.GetAssetsPath(), localPrefix ).Replace( '\\', '/' );
		MaterialTextureList textures = await GetTextures();

		Log.Info( $"Downloading textures to {globalPrefix}..." );

		List<Task<string>> DownloadTasks = new( 5 );
		DownloadedFileList fileList = new();

		FileReference? diff = textures.Diffuse?.GetResolution(resolution)?.JPEG;
		if ( diff.HasValue )
			DownloadTasks.Add( CreateDownloadTask( diff.Value, fileList.SetDiffuse, localPrefix, globalPrefix, "color.jpg" ) );

		FileReference? normal = textures.NormalGL?.GetResolution(resolution)?.PNG;
		if ( normal.HasValue )
			DownloadTasks.Add( CreateDownloadTask( normal.Value, fileList.SetNormal, localPrefix, globalPrefix, "normal.png" ) );

		// Don't need any higher resolution
		FileReference? height = textures.Displacement?.Res1k?.PNG;
		if ( height.HasValue )
			DownloadTasks.Add( CreateDownloadTask( height.Value, fileList.SetHeight, localPrefix, globalPrefix, "height.png" ) );

		FileReference? ao = textures.AO?.GetResolution(aoRes)?.JPEG;
		if ( ao.HasValue )
			DownloadTasks.Add( CreateDownloadTask( ao.Value, fileList.SetAO, localPrefix, globalPrefix, "ao.jpg" ) );

		FileReference? rough = textures.Rough?.GetResolution(resolution)?.PNG;
		if ( rough.HasValue )
			DownloadTasks.Add( CreateDownloadTask( rough.Value, fileList.SetRough, localPrefix, globalPrefix, "rough.png" ) );

		FileReference? metal = textures.Metal?.GetResolution(resolution)?.JPEG;
		if ( metal.HasValue )
			DownloadTasks.Add( CreateDownloadTask( metal.Value, fileList.SetMetal, localPrefix, globalPrefix, "metal.jpg" ) );

		await Task.WhenAll( DownloadTasks );
		DownloadedFiles = fileList;
		return fileList;
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
	public void GenerateSBoxAsset( bool useDisplacement = false )
	{
		if ( DownloadedFiles == null )
			throw new InvalidOperationException( "Textures have not been downloaded." );

		if ( SBoxAsset != null )
			Log.Warning( "The asset has already been generated. Overridding..." );

		var activeProject = PolySettings.Instance.ActiveProject;
		if ( activeProject == null )
			throw new InvalidOperationException( "no active project." );

		var vmatPath = $"{LocalBasePath}/{PolyHavenID}.vmat";
		Log.Info( "Generating material " + vmatPath );

		Template template;
		if ( DownloadedFiles.Metal != null )
			template = new Template( "templates/simple_metal.template" );
		else
			template = new Template( "templates/simple_standard.template" );

		var vmatContents = template.Parse( new Dictionary<string, string?>
		{
			{ "ColorTexture", DownloadedFiles.Color },
			{ "RoughnessTexture", DownloadedFiles.Rough },
			{ "NormalTexture", DownloadedFiles.Normal },
			{ "AOTexture", DownloadedFiles.AO },
			{ "MetalnessTexture", DownloadedFiles.Metal }
		} );

		File.WriteAllText( Path.Combine( activeProject.GetAssetsPath(), vmatPath ), vmatContents );
		AssetSystem.RegisterFile( Path.Combine( activeProject.GetAssetsPath(), vmatPath ) );

		SBoxAsset = AssetSystem.FindByPath( vmatPath );
		Log.Info( $"Wrote material to {SBoxAsset}" );

		SBoxAsset.Compile( true );
	}


	public void SetupMetadata()
	{
		if ( SBoxAsset == null )
			throw new InvalidOperationException( "Material has not been generated." );

		SBoxAsset.MetaData.Set( "polyhaven_id", PolyHavenID );
		SBoxAsset.Publishing.CreateTemporaryProject();

		// Indents may not be longer than 32 characters.
		string indent = PolyHavenID;
		if ( indent.Length > 32 )
		{
			indent = new Random().NextStrings( 16, 1, allowedChars: RandomCharacters.INDENT_ALLOWED_CHARACTERS ).First();
		}
		var tags = new HashSet<string>();
		foreach ( var tag in Asset.Tags )
			tags.Add( tag );
		foreach ( var tag in Asset.Categories )
			tags.Add( tag );

		SBoxAsset.Publishing.ProjectConfig.Ident = indent;
		SBoxAsset.Publishing.ProjectConfig.Title = Asset.Name;
		SBoxAsset.Publishing.ProjectConfig.Tags = string.Join( ' ', ReplaceSpaces( tags ) );
		SBoxAsset.Publishing.ProjectConfig.Org = "polyhaven";
		SBoxAsset.Publishing.Save();

		SBoxAsset.MetaData.Set( "PolyAsset", Asset );
	}

	private IEnumerable<string> ReplaceSpaces( IEnumerable<string> src )
	{
		foreach ( string s in src )
		{
			yield return s.Replace( " ", "" );
		}
	}

	public override string ToString()
	{
		return $"TextureMaterialAsset[{PolyHavenID}]";
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
