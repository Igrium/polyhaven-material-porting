using PolyHaven.API;
using PolyHaven.Assets;
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

		var thumbTask = ThumbnailGenerator.GenerateThumbnail( asset );
		var mat = asset.GenerateMaterial();
		var compileTask = CompileAsync( mat );

		await Task.WhenAll( thumbTask, compileTask );

		asset.SetupMetadata();

		if ( publish )
			await AssetPublishing.Publish( asset );

		Log.Info( "Skybox generation complete!" );
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
