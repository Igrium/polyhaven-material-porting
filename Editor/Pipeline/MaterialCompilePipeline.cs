using PolyHaven.API;
using PolyHaven.Assets;
using System;
using System.Threading.Tasks;
namespace PolyHaven.Pipeline;

/// <summary>
/// Materials are their own thing - they're a set of texture maps sorted into a category folder, with
/// no skybox thumbnail to render. The skybox pipeline in <see cref="AssetCompilePipeline"/> doesn't
/// apply to them.
/// </summary>
public class MaterialCompilePipeline
{
	public async Task SetupAsset( string id, string resolution = "2k", string aoResolution = "1k", bool texOnly = false, PolyHavenAssetInfo? info = null )
	{
		Log.Info( $"Downloading up {id} with a resolution of {resolution}" );
		Log.Info( "Retrieving asset metadata" );

		info ??= await PolyHavenApi.GetAsset( id );
		if ( info == null || info.Type != PhAssetType.Texture )
		{
			throw new ArgumentException( "The supplied asset must be a material.", nameof( id ) );
		}

		Log.Info( "Downloading textures..." );
		TextureMaterialAsset asset = new TextureMaterialAsset( id, info );
		await asset.DownloadFiles( resolution, aoResolution );

		if ( !texOnly )
		{
			// Back to the main thread - the asset system asserts it.
			await MainThread.Wait();

			var mat = asset.GenerateMaterial();
			await AssetCompilePipeline.CompileAsync( mat );

			asset.SetupMetadata();
		}
		else
		{
			Log.Info( "Supressing material generation." );
		}
	}
}
