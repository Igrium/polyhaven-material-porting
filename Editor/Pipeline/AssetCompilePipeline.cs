using PolyHaven.API;
using PolyHaven.AssetParty;
using PolyHaven.Assets;
using PolyHaven.Meta;
using System;
using System.IO;
using System.Threading.Tasks;
namespace PolyHaven.Pipeline;

public static class AssetCompilePipeline
{
	public static async Task DoCompile( string id, PolyHavenAssetInfo? info = null, bool publish = true )
	{
		info ??= await PolyHavenApi.GetAsset( id );
		if ( info == null )
			throw new ArgumentException( "Cannot find an asset with that ID.", nameof( id ) );

		if ( info.Type != PhAssetType.HDRI )
			throw new ArgumentException( $"{info.Type} assets don't go through the skybox pipeline - see MaterialCompilePipeline.", nameof( id ) );

		await CompileHdri( id, info, publish );
	}

	private static async Task CompileHdri( string id, PolyHavenAssetInfo info, bool publish )
	{
		var asset = new HdriAsset( id, info );
		await asset.DownloadHdr();

		// The download continuation lands on a threadpool thread, and the asset system is main-thread
		// only - AssetSystem.RegisterFile asserts it.
		await MainThread.Wait();

		// Blender is a separate process, so let it render while the material compiles.
		var thumbTask = ThumbnailGenerator.GenerateThumbnail( asset );
		var mat = asset.GenerateMaterial();
		var compileTask = CompileAsync( mat );

		var thumbPath = await thumbTask;
		await compileTask;

		// Assign the thumbnail last. Compiling the asset makes the engine rebuild its thumbnail, and
		// that render happily replaces an override set before it with the flat equirect preview it
		// draws for a sky material.
		await MainThread.Wait();
		ThumbnailGenerator.AssignThumbnail( mat, thumbPath );

		asset.SetupMetadata();

		if ( publish )
		{
			await AssetPublishing.Publish( asset );

			// Deliberately outside the dry-run check in Publish - the whole point of the metadata
			// server is filling in descriptions by hand, and that wants to work while uploads are
			// stubbed out. AssetPartyURL just comes back null until a real publish has happened.
			await MetaServer.Send( new AssetMeta( asset ) );
		}

		Log.Info( "Skybox generation complete!" );
	}

	/// <summary>
	/// Query Poly Haven and Asset Party and return all the hdris that haven't been ported yet.
	/// </summary>
	/// <returns>A map of asset ids and their relevant entries.</returns>
	public static async Task<IDictionary<string, PolyHavenAssetInfo>> GetUnfinishedAssets()
	{
		var assets = await PolyHavenApi.GetAssets( "hdris" );
		var uploadedAssets = await AssetPartyProxy.ExistingPackages();

		var filteredAssets = new Dictionary<string, PolyHavenAssetInfo>();
		foreach ( var kv in assets )
		{
			if ( !await AssetPartyProxy.PackageExists( kv.Key, uploadedAssets ) )
				filteredAssets.Add( kv.Key, kv.Value );
		}

		return filteredAssets;
	}

	private static bool ShouldStop = false;

	public static async Task DoMassCompile()
	{
		ShouldStop = false;
		Log.Info( "Starting mass compile." );
		Log.Info( "Searching for unported assets..." );

		var assets = await GetUnfinishedAssets();
		Log.Info( $"Found {assets.Count} assets" );

		foreach ( var asset in assets )
		{
			if ( ShouldStop )
			{
				Log.Info( "Mass compile stopped." );
				return;
			}

			if ( asset.Key.Length > 32 || asset.Value.Name.Length > 32 )
			{
				Log.Warning( $"The title or ID for '{asset.Key}' is longer than 32 characters. Skipping." );
				continue;
			}

			try
			{
				Log.Info( $"Beginning port for {asset.Key}" );
				await DoCompile( asset.Key, asset.Value );
			}
			catch ( Exception ex )
			{
				await ErrorHandling.WriteError( asset.Key, ex );
				Log.Error( $"Error converting {asset.Key}:" );
				Log.Error( ex );
			}
		}

		Log.Info( "Mass compile complete." );
	}

	public static void StopCompile()
	{
		ShouldStop = true;
	}

	// wrap in a task
	internal static async Task<bool> CompileAsync( Asset asset )
	{
		await MainThread.Wait();

		// Forced compile, like the original did. CompileIfNeededAsync would be the tidier call, but it
		// polls IsCompiled until it flips - and on Linux it never does, for the reason below.
		var result = asset.Compile( true );

		RegisterCompiledOutput( asset );
		return result;
	}

	/// <summary>
	/// CDirWatcher is a no-op on Linux, so the asset system never notices the files the resource
	/// compiler just wrote next to the source. The asset keeps reading as uncompiled and loading it
	/// fails with ERROR_FILEOPEN until the editor restarts. Hand the compiled files over explicitly.
	/// </summary>
	private static void RegisterCompiledOutput( Asset asset )
	{
		var source = asset.GetSourceFile( true );
		if ( string.IsNullOrEmpty( source ) )
			return;

		var dir = Path.GetDirectoryName( source );
		if ( dir == null )
			return;

		// The textures the material compiler generated for us. They sit beside the source texture -
		// which is this folder for a skybox, and the tex_<id> subfolder for a material - and the vmat_c
		// won't load without them ("Parent with missing children").
		foreach ( var child in Directory.EnumerateFiles( dir, "*.generated.vtex_c", SearchOption.AllDirectories ) )
			AssetSystem.RegisterFile( child );

		var compiled = source + "_c";
		if ( File.Exists( compiled ) )
			AssetSystem.RegisterFile( compiled );
	}
}
