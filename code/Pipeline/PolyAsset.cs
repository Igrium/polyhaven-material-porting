﻿using Editor;
using PolyHaven.API;
using PolyHaven.Util;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Pipeline;
public class PolyAsset
{
	public string PolyHavenID { get; init; }
	public AssetEntry Asset { get; init; }
	public string? SourceTexturePath { get; protected set; }
	public Asset? Material { get; protected set; }

	public PolyAsset( string polyHavenID, AssetEntry asset )
	{
		PolyHavenID = polyHavenID;
		Asset = asset;

	}

	public async Task<string> DownloadHDR( string resolution = "4k" )
	{
		var activeProject = PolySettings.Instance.ActiveProject;
		if ( activeProject == null )
			throw new InvalidOperationException( "no active project." );

		var resolutions = await ApiManager.Instance.GetHDRFiles( PolyHavenID );

		if ( !resolutions.TryGetValue( resolution, out var fileRef ) )
		{
			throw new ArgumentException( "Unknown resolution: " + resolution, nameof( resolution ) );
		}

		string fileDest = Path.Combine( activeProject.GetAssetsPath(), "materials", "skybox", PolyHavenID + ".exr" );
		Log.Info( "Downloading file from " + fileRef.URL );

		bool success = await Utility.DownloadAsync( fileRef.URL, fileDest );
		if ( !success )
		{
			throw new InvalidOperationException( $"Download of {PolyHavenID} failed." );
		}

		fileDest = Path.GetRelativePath( activeProject.GetAssetsPath(), fileDest ).Replace('\\', '/');
		SourceTexturePath = fileDest;
		Log.Info( $"Saved file to {activeProject.Package.Ident}.{fileDest}" );

		return fileDest;
	}

	public void GenerateMaterial()
	{
		var activeProject = PolySettings.Instance.ActiveProject;
		if ( activeProject == null )
			throw new InvalidOperationException( "no active project." );

		if ( SourceTexturePath == null )
			throw new InvalidOperationException( "Source texture has not been installed." );

		var vmatPath = $"materials/skybox/{PolyHavenID}.vmat";
		Log.Info( "Generating material " + vmatPath );

		var template = new Template( "templates/skybox.template" );


		var vmatContents = template.Parse( new Dictionary<string, string>() { { "SkyTexture", SourceTexturePath } } );
		File.WriteAllText( Path.Combine( activeProject.GetAssetsPath(), vmatPath ), vmatContents );

		AssetSystem.RegisterFile( Path.Combine( activeProject.GetAssetsPath(), vmatPath ) );

		Material = Editor.AssetSystem.FindByPath( vmatPath );
		Log.Info( $"Wrote material to {Material}. Compiling..." );

		Material.Compile(true);
		Log.Info( "Material compilation complete." );
	}

	public void SetupMetadata()
	{
		if ( Material == null )
			throw new InvalidOperationException( "Material has not been generated." );

		Material.MetaData.Set( "polyhaven_id", PolyHavenID );
		Material.Publishing.CreateTemporaryProject();

		Material.Publishing.ProjectConfig.Ident = PolyHavenID;
		Material.Publishing.ProjectConfig.Title = Asset.Name;
		Material.Publishing.ProjectConfig.Tags = string.Join( ' ', ReplaceSpaces( Asset.Tags ) );
		Material.Publishing.Save();
	}

	public async void Publish()
	{
		Log.Info( "Setting up asset publish" );
		var publisher = await ProjectPublisher.FromAsset( Material );
		publisher.SetMeta( "polyhaven_id", PolyHavenID );
	}

	private IEnumerable<string> ReplaceSpaces(IEnumerable<string> src)
	{
		foreach ( string s in src )
		{
			yield return s.Replace( ' ', '_' );
		}
	}

	/// <summary>
	/// Create a poly asset from a polyhaven id, scraping the metadata from PolyHaven.
	/// </summary>
	/// <param name="polyHavenID">The ID</param>
	/// <returns>The entry</returns>
	/// <exception cref="ArgumentException">If the ID cannot be found.</exception>
	public static async Task<PolyAsset> Create( string polyHavenID )
	{
		var assets = await ApiManager.Instance.GetAssets( "hdris" );

		if ( assets.TryGetValue( polyHavenID, out var entry ) )
		{
			return new PolyAsset( polyHavenID, entry );
		}
		else
		{
			throw new ArgumentException( "Unknown polyhaven ID: " + polyHavenID, nameof( polyHavenID ) );
		}
	}
}