using PolyHaven.API;
using System.IO;
using System.Threading.Tasks;
namespace PolyHaven;

public static class Commands
{
	[ConCmd("ph_test")]
	public static void TestCommand()
	{
		Log.Info("TestCommand");
	}

	[ConCmd("ph_dump_assets")]
	public static async Task DumpAssets()
	{
		var result = await PolyHavenApi.GetAssets( "models" );
		
		// foreach ( var entry in result )
		// {
		// 	Log.Info( $"{{{entry.Key}, {entry.Value}}}" );
		// }
		Log.Info( result );
	}

	[ConCmd("ph_download_hdri")]
	public static async Task DownloadHdri( string id )
	{
		var asset = await PolyHavenApi.GetHdriAsset( id );
		await asset.DownloadHdr();
		var mat = asset.GenerateMaterial();
		await mat.CompileIfNeededAsync();
		Log.Info("Asset has been compiled!");
	}
}
