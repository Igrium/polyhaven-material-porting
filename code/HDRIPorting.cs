using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using PolyHaven.API;
using Sandbox;
using Sandbox.Diagnostics;

namespace PolyHaven;

public static class HDRIPorting
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

}
