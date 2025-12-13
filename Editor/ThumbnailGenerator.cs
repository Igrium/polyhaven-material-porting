using PolyHaven.Assets;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FileSystem = Editor.FileSystem;
namespace PolyHaven;

public class ThumbnailGenerator
{
	public static async Task<string> GenerateThumbnail( HdriAsset asset )
	{
		if ( asset.SourceTexturePath == null )
			throw new InvalidOperationException( "Source texture has not been downloaded." );
		var outPath = await GenerateThumbnail( asset.SourceTexturePath, asset.PolyHavenId );
		if (asset.SBoxAsset != null)
			AssignThumbnail(asset.SBoxAsset, outPath);
		return outPath;
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
		return outputPath;
	}

	public static void AssignThumbnail( Asset asset, string thumbPath )
	{
		var fullPath = FileSystem.Content.GetFullPath( thumbPath );
		if ( fullPath == null )
		{
			Log.Warning( "Unable to find thumbnail: " + thumbPath );
		}
		Pixmap thumbnail = Pixmap.FromFile( fullPath );
		asset.OverrideThumbnail( thumbnail );
	}

	protected static async Task RunBlenderProcess( string exrInput, string imageOutput, string blendFile )
	{
		string blenderPath = Commands.BlenderPath;
		Log.Info( $"loading exr {exrInput} into blender at {blenderPath}" );
		if ( !File.Exists( blenderPath ) )
			throw new InvalidOperationException( "Please set ph_blender_path first" );

		var scriptFile = Path.Join( Path.Join( Project.Current.GetRootPath(), "python/render_thumbnail.py" ) );

		string[] arguments = ["-b", blendFile, "--python", scriptFile, "--exr", exrInput, "--output", imageOutput];
		
		var process = new Process();
		process.StartInfo = new ProcessStartInfo( blenderPath, arguments );

		var result = await RunProcessAsync( process );
		Log.Info( "Blender process closed with return value " + result );
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
