using PolyHaven.API;
using PolyHaven.Pipeline;
using System;
using System.Threading.Tasks;
namespace PolyHaven;

public static class Commands
{
	[ConCmd( "ph_test" )]
	public static void TestCommand()
	{
		Log.Info( "TestCommand" );
	}

	[ConCmd( "ph_dump_assets" )]
	public static async Task DumpAssets()
	{
		var result = await PolyHavenApi.GetAssets( "models" );

		// foreach ( var entry in result )
		// {
		// 	Log.Info( $"{{{entry.Key}, {entry.Value}}}" );
		// }
		Log.Info( result );
	}

	[ConCmd( "ph_download_hdri" )]
	public static Task DownloadHdri( string id )
	{
		return Port( id );
	}

	/// <summary>
	/// Runs the full HDRI pipeline including publish. Publishing itself still respects
	/// AssetPublishing.DryRun, so this is safe to run without side effects until that's flipped off.
	/// </summary>
	[ConCmd( "ph_test_hdri_pipeline" )]
	public static Task TestHdriPipeline( string id )
	{
		return Guard( id, () => AssetCompilePipeline.DoCompile( id, null, true ) );
	}

	[ConCmd( "ph_download_texture" )]
	public static Task DownloadTexture( string id, string res = "2k", string aoRes = "1k" )
	{
		return Guard( id, () => new MaterialCompilePipeline().SetupAsset( id, res, aoRes ) );
	}

	/// <summary>
	/// An exception thrown out of an async ConCmd goes nowhere - the console shows nothing at all -
	/// so catch it here and log it.
	/// </summary>
	private static Task Port( string id )
	{
		return Guard( id, () => AssetCompilePipeline.DoCompile( id, null, false ) );
	}

	private static async Task Guard( string id, Func<Task> work )
	{
		try
		{
			await work();
		}
		catch ( Exception e )
		{
			Log.Error( e, $"Failed to port '{id}': {e.Message}" );
		}
	}

	[ConVar( "ph_blender_path", ConVarFlags.Saved )]
	public static string BlenderPath { get; set; } = "";
}
