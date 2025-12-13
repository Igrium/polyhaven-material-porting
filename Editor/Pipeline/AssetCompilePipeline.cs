using PolyHaven.API;
using PolyHaven.Assets;
using System;
using System.Threading.Tasks;
namespace PolyHaven.Pipeline;

public static class AssetCompilePipeline
{
	public static async Task DoCompile( string id, PolyHavenAssetInfo? info = null, bool publish = true )
	{
		if ( info == null )
		{
			info = await PolyHavenApi.GetAsset( id );
			if ( info == null || info.Type != PhAssetType.HDRI )
				throw new ArgumentException( "The supplied asset must be an HDRI.", nameof( id ) );
		}

		var asset = new HdriAsset( id, info );
		await asset.DownloadHdr();
		
		var thumbTask = ThumbnailGenerator.GenerateThumbnail( asset );
		var mat = asset.GenerateMaterial();
		var compileTask = CompileAsync( mat );
		
		await Task.WhenAll( thumbTask, compileTask );
		
		Log.Info("Skybox generation complete!");
	}
	
	// wrap in a task
	private static async Task<bool> CompileAsync( Asset asset )
	{
		return await asset.CompileIfNeededAsync();
	}
}
