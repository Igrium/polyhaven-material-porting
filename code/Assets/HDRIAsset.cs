using Editor;
using PolyHaven.API;
using PolyHaven.Util;
using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Assets;
public class HDRIAsset : IPolyAsset
{
	public string PolyHavenID { get; init; }
	public AssetEntry Asset { get; init; }
	public string? SourceTexturePath { get; protected set; }
	public Asset? SBoxAsset { get; protected set; }
	public string? AssetPartyURL { get; set; }

	public HDRIAsset( string polyHavenID, AssetEntry asset )
	{
		PolyHavenID = polyHavenID;
		Asset = asset;
		asset.Tags.Add( "skybox" );

	}

	public async Task<string> DownloadHDR( string resolution = "4k" )
	{
		var activeProject = PolySettings.Instance.ActiveProject;
		if ( activeProject == null )
			throw new InvalidOperationException( "no active project." );

		var resolutions = await PolyHavenAPI.Instance.GetHDRFiles( PolyHavenID );

		if ( !resolutions.TryGetValue( resolution, out var fileRef ) )
		{
			throw new ArgumentException( "Unknown resolution: " + resolution, nameof( resolution ) );
		}

		string fileDest = Path.Combine( activeProject.GetAssetsPath(), "materials", "skybox", PolyHavenID + ".exr" );
		Log.Info( "Downloading file from " + fileRef.URL );

		bool success = await Utility.DownloadAsync( fileRef.URL, fileDest );
		if ( !success )
		{
			throw new InvalidOperationException( $"Download of {PolyHavenID} failed." );
		}

		fileDest = Path.GetRelativePath( activeProject.GetAssetsPath(), fileDest ).Replace( '\\', '/' );
		SourceTexturePath = fileDest;
		Log.Info( $"Saved file to {activeProject.Package.Ident}.{fileDest}" );

		return fileDest;
	}

	public async Task<IEnumerable<string>> DownloadFiles()
	{
		var file = await DownloadHDR();
		return new string[] { file };
	}

	public void GenerateHDRMaterial()
	{
		var activeProject = PolySettings.Instance.ActiveProject;
		if ( activeProject == null )
			throw new InvalidOperationException( "no active project." );

		if ( SourceTexturePath == null )
			throw new InvalidOperationException( "Source texture has not been installed." );

		var vmatPath = $"materials/skybox/{PolyHavenID}.vmat";
		Log.Info( "Generating material " + vmatPath );

		var template = new Template( "templates/skybox.template" );


		var vmatContents = template.Parse( new Dictionary<string, string>() { { "SkyTexture", SourceTexturePath } } );
		File.WriteAllText( Path.Combine( activeProject.GetAssetsPath(), vmatPath ), vmatContents );

		AssetSystem.RegisterFile( Path.Combine( activeProject.GetAssetsPath(), vmatPath ) );

		SBoxAsset = AssetSystem.FindByPath( vmatPath );
		Log.Info( $"Wrote material to {SBoxAsset}. Compiling..." );

		SBoxAsset.Compile( true );
		Log.Info( "Material compilation complete." );
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
	}

	private IEnumerable<string> ReplaceSpaces( IEnumerable<string> src )
	{
		foreach ( string s in src )
		{
			yield return s.Replace( " ", "" );
		}
	}

	/// <summary>
	/// Create a poly asset from a polyhaven id, scraping the metadata from PolyHaven.
	/// </summary>
	/// <param name="polyHavenID">The ID</param>
	/// <returns>The entry</returns>
	/// <exception cref="ArgumentException">If the ID cannot be found.</exception>
	public static async Task<HDRIAsset> Get( string polyHavenID )
	{
		AssetEntry? entry = await PolyHavenAPI.Instance.GetAsset( polyHavenID );
		if ( entry == null || entry.Type != API.AssetType.HDRI )
			throw new ArgumentException( "The supplied asset must be an HDRI.", nameof( polyHavenID ) );

		return new HDRIAsset( polyHavenID, entry );
	}
}
