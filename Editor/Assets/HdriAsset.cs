using PolyHaven.API;
using PolyHaven.Util;
using System;
using System.IO;
using System.Threading.Tasks;
namespace PolyHaven.Assets;

public class HdriAsset : IPolyAsset
{
	public string PolyHavenId { get; init; }
	public PolyHavenAssetInfo Info { get; init; }
	public string? SourceTexturePath { get; protected set; }
	public Asset? SBoxAsset { get; protected set; }

	public HdriAsset( string polyHavenId, PolyHavenAssetInfo info )
	{
		PolyHavenId = polyHavenId;
		Info = info;
		info.Tags.Add( "skybox" );
	}

	public async Task<string> DownloadHdr( string resolution = "4k" )
	{
		var activeProject = PortingUtility.RequireProject();
		var resolutions = await PolyHavenApi.GetHdrFiles( PolyHavenId );

		if ( !resolutions.TryGetValue( resolution, out var fileRef ) )
		{
			throw new ArgumentException( "Unknown resolution: " + resolution, nameof( resolution ) );
		}

		string fileDest = Path.Combine( activeProject.GetAssetsPath(), "materials", "skybox", PolyHavenId + ".exr" );
		Log.Info( "Downloading file from " + fileRef.Url );

		bool success = await fileRef.DownloadAsync( fileDest );
		if ( !success )
		{
			throw new InvalidOperationException( $"Download of {PolyHavenId} failed." );
		}

		fileDest = Path.GetRelativePath( activeProject.GetAssetsPath(), fileDest ).Replace( '\\', '/' );
		SourceTexturePath = fileDest;
		Log.Info( $"Saved file to {activeProject.Config.Ident}.{fileDest}" );

		return fileDest;
	}

	public async Task<IEnumerable<string>> DownloadFiles()
	{
		var file = await DownloadHdr();
		return new string[] { file };
	}

	public Asset GenerateMaterial()
	{
		var activeProject = PortingUtility.RequireProject();

		if ( SourceTexturePath == null )
			throw new InvalidOperationException( "Source texture has not been installed." );

		// The .exr came down over plain http, so the asset system has never heard of it. Register it
		// before the material that references it, or the vmat compile fails with
		// "Invalid Dependency Information".
		AssetSystem.RegisterFile( Path.Combine( activeProject.GetAssetsPath(), SourceTexturePath ) );

		var vmatPath = $"materials/skybox/{PolyHavenId}.vmat";
		Log.Info( "Generating material " + vmatPath );

		var template = new Template( "skybox.template" );

		var vmatContents = template.Parse( new Dictionary<string, string?>() { { "SkyTexture", SourceTexturePath } } );

		SBoxAsset = PortingUtility.WriteAndRegisterMaterial( vmatPath, vmatContents );
		return SBoxAsset;
	}

	public void SetupMetadata()
	{
		PortingUtility.ApplyStandardMetadata( this, "CC0" );
	}

	public override string ToString() => $"HdriAsset[{PolyHavenId}]";
}
