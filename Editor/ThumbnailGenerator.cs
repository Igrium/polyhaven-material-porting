using PolyHaven.Assets;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
namespace PolyHaven;

public class ThumbnailGenerator
{
	public static Task<string> GenerateThumbnail( HdriAsset asset )
	{
		if ( asset.SourceTexturePath == null )
			throw new InvalidOperationException( "Source texture has not been downloaded." );

		return GenerateThumbnail( asset.SourceTexturePath, asset.PolyHavenId );
	}

	public static async Task<string> GenerateThumbnail( string exrPath, string name )
	{
		Log.Info( "Generating thumbnail for " + name );
		var activeProject = Project.Current;

		string globalExrPath = Path.Combine( activeProject.GetAssetsPath(), exrPath );
		string outputPath = $"materials/skybox/thumbnails/{name}.png";
		string globalOutputPath = Path.Combine( activeProject.GetAssetsPath(), outputPath );

		var blendFile = Path.Join( Project.Current.GetRootPath(), "skies.blend" );

		await RunBlenderProcess( globalExrPath, globalOutputPath, blendFile );

		Log.Info( "Saved thumbnail to " + outputPath );

		// Absolute, not content-relative. Blender wrote straight to disk and CDirWatcher does nothing
		// on Linux, so FileSystem.Content can't resolve the path it just created.
		return globalOutputPath;
	}

	public static void AssignThumbnail( Asset asset, string thumbPath )
	{
		// Pixmap and OverrideThumbnail are native calls - they won't survive a threadpool thread.
		ThreadSafe.AssertIsMainThread();

		if ( !File.Exists( thumbPath ) )
		{
			Log.Warning( $"Blender reported success but '{thumbPath}' isn't there - leaving {asset} with the thumbnail the engine renders itself." );
			return;
		}

		// Not Pixmap.FromFile - it prefixes anything without a colon in it with the "toolimages:" Qt
		// search path, which is every absolute path on Linux, and hands back a pixmap that reports
		// itself fine but fails to save. Decode the bytes ourselves instead.
		using var bitmap = Bitmap.CreateFromBytes( File.ReadAllBytes( thumbPath ) );
		if ( bitmap == null )
		{
			Log.Warning( $"Couldn't decode thumbnail '{thumbPath}' - leaving {asset} with the thumbnail the engine renders itself." );
			return;
		}

		Pixmap thumbnail = Pixmap.FromBitmap( bitmap );
		if ( thumbnail == null )
		{
			Log.Warning( $"Couldn't read thumbnail '{thumbPath}' - leaving {asset} with the thumbnail the engine renders itself." );
			return;
		}

		// This only sets Asset.thumbnailOverride, which lives on the Asset object and isn't written
		// anywhere. The engine persists it for us on the rebuild OverrideThumbnail queues: that render
		// returns the override and saves it to the thumbnail cache. Anything that makes the engine
		// rebuild the thumbnail afterwards - compiling the asset, for one - can undo it, so this wants
		// to be the last thing the pipeline does to the asset.
		asset.OverrideThumbnail( thumbnail );
		Log.Info( $"Assigned thumbnail {thumbPath} to {asset}" );
	}

	protected static async Task RunBlenderProcess( string exrInput, string imageOutput, string blendFile )
	{
		string blenderPath = Commands.BlenderPath;
		Log.Info( $"loading exr {exrInput} into blender at {blenderPath}" );
		if ( !File.Exists( blenderPath ) )
			throw new InvalidOperationException( "Please set ph_blender_path first" );

		var scriptFile = Path.Join( Path.Join( Project.Current.GetRootPath(), "python/render_thumbnail.py" ) );

		// Everything after "--" is ours - without it Blender tries to open "--exr" as a .blend and errors.
		string[] arguments = ["-b", blendFile, "--python", scriptFile, "--", "--exr", exrInput, "--output", imageOutput];
		
		var process = new Process();
		process.StartInfo = new ProcessStartInfo( blenderPath, arguments );

		var result = await RunProcessAsync( process );
		Log.Info( "Blender process closed with return value " + result );

		if ( result != 0 )
			throw new InvalidOperationException( $"Blender exited with code {result} - no thumbnail was written." );
	}

	/// <summary>
	/// Execute a process asynchronously
	/// </summary>
	/// <param name="process">The process</param>
	/// <returns>The exit code</returns>
	private static Task<int> RunProcessAsync( Process process )
	{
		var tsc = new TaskCompletionSource<int>();
		process.EnableRaisingEvents = true;

		process.Exited += ( sender, args ) =>
		{
			tsc.SetResult( process.ExitCode );
			process.Dispose();
		};

		process.Start();
		return tsc.Task;
	}
}
