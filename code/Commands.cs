using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Editor;
using PolyHaven.API;
using PolyHaven.Pipeline;
using Sandbox;
using Sandbox.Diagnostics;

namespace PolyHaven;

public static class Commands
{
	public static ApiManager APIManager { get; set; } = new ApiManager();

	[ConCmd.Engine("test_request")]
	public async static void TestRequest()
	{
		HttpClient client = new()
		{
			BaseAddress = new Uri( "https://api.polyhaven.com" )
		};

		//var res = await client.GetAsync( "/assets?t=hdris" );
		//res.EnsureSuccessStatusCode();

		var dict = await APIManager.GetAssets( "models" );

		foreach (var entry in dict )
		{
			Log.Info( $"{{{entry.Key}, {entry.Value}}}" );
		}
		client.Dispose();
	}

	[ConCmd.Engine("print_polyfiles")]
	public async static void PrintAllFiles(string category = "hdris")
	{
		var assets = await APIManager.GetAssets( category );
		foreach ( var asset in assets )
		{
			GetAndPrintFile( asset.Key );
		}
	}

	private async static void GetAndPrintFile(string id)
	{
		var files = await APIManager.GetHDRFiles( id );
		Log.Info( files["4k"].URL );
	}

	[ConCmd.Engine("polyhaven_file")]
	public async static void GetFiles(string id)
	{

		var dict = await APIManager.GetHDRFiles( id );

		foreach ( var entry in dict )
		{
			Log.Info( $"{{{entry.Key}, {entry.Value}}}" );
		}
	}

	[ConCmd.Engine("print_active_project")]
	public static void PrintActiveProject()
	{
		PolySettings.Instance.ReloadActiveProject();
		Log.Info( PolySettings.Instance.ActiveProject );
	}

	[ConCmd.Engine("polyhaven_reload")]
	public static void ReloadConfig()
	{
		PolySettings.Reload();
		Log.Info( "Settings reloaded." );
	}

	[ConCmd.Engine("polyhaven_download")]
	public async static void TestDownload(string id)
	{
		var asset = await PolyAsset.Create( id );
		await asset.DownloadHDR();
	}

	[ConCmd.Engine("polyhaven_compile")]
	public async static void TestCompile(string id)
	{
		var asset = await PolyAsset.Create( id );
		await asset.DownloadHDR();
		asset.GenerateMaterial();
		asset.SetupMetadata();
		await asset.Publish();
	}

	[ConCmd.Engine("list_files")]
	public static void TestFile()
	{
		Log.Info( "showing files" );
		string files = "";
		foreach(var file in FileSystem.Content.FindFile( "", recursive: true ))
		{
			files += file + '\n';
		}
		FileSystem.Root.WriteAllText( "assets.txt", files );
		Log.Info( "Wrote to " + FileSystem.Root.GetFullPath( "assets.txt" ));
	}
}
