﻿using Editor;
using PolyHaven.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Pipeline;

public class ThumbnailGenerator
{
	public static Task<string> GenerateThumbnail( PolyAsset asset )
	{
		if ( asset.SourceTexturePath == null )
			throw new InvalidOperationException( "Source texture has not been downloaded." );
		return GenerateThumbnail( asset.SourceTexturePath, asset.PolyHavenID );
	}

	public static async Task<string> GenerateThumbnail( string exrPath, string name )
	{
		Log.Info( "Generating thumbnail for " + name );
		var activeProject = PolySettings.Instance.ActiveProject;
		if ( activeProject == null )
			throw new InvalidOperationException( "No active project." );

		string globalExrPath = Path.Combine( activeProject.GetAssetsPath(), exrPath );
		string outputPath = $"materials/skybox/thumbnails/{name}.jpg";
		string globalOutputPath = Path.Combine( activeProject.GetAssetsPath(), outputPath );

		var blendFile = FileSystem.Content.GetFullPath( "skies.blend" );
		if ( blendFile == null )
			throw new InvalidOperationException( "Unable to find skies.blend" );

		await RunBlenderProcess( globalExrPath, globalOutputPath, blendFile );

		Log.Info( "Saved thumbnail to " + outputPath );
		return outputPath;
	}

	protected static async Task RunBlenderProcess( string exrInput, string imageOutput, string blendFile )
	{
		string blenderPath = PolySettings.Instance.BlenderPath;
		Log.Info( "exr input: " + exrInput );
		if ( !File.Exists( blenderPath ) )
			throw new InvalidOperationException( "Please set your Blender path in " + PolySettings.SETTINGS_FILE );

		var scriptFile = FileSystem.Content.GetFullPath( "python/render_thumbnail.py" );
		if ( scriptFile == null )
		{
			throw new InvalidOperationException( "Unable to locate rendery_thumbnail.py" );
		}

		var process = new Process();
		process.StartInfo.FileName = blenderPath;
		process.StartInfo.Arguments = $"-b \"{blendFile}\" --python \"{scriptFile}\" --exr \"{exrInput}\" --output \"{imageOutput}\"";
		process.StartInfo.UseShellExecute = true;

		var result = await ProcessLauncher.RunProcessAsync( process );
		Log.Info( "Blender process closed with return value " + result );
	}
}