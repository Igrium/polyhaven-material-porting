using PolyHaven.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using System.Net.Http;
using System.Text.Json;
using Editor;

namespace PolyHaven.Meta;

public static class MetaServer
{
	public static HttpClient HttpClient { get; private set; } = new()
	{
		Timeout = TimeSpan.FromSeconds( 1 )
	};

	public static async void Send( AssetMeta asset )
	{
		string json = JsonSerializer.Serialize( asset );
		var content = new StringContent( json, Encoding.UTF8, "application/json" );
		var response = await HttpClient.PostAsync( "http://localhost:8080/submit", content );
		if ( !response.IsSuccessStatusCode )
		{
			Log.Error( "Metadata server returned response code: " + response.StatusCode );
		}
	}
}
