using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace PolyHaven.Meta;

public static class MetaServer
{
	public static HttpClient HttpClient { get; } = new()
	{
		Timeout = TimeSpan.FromSeconds( 1 )
	};

	/// <summary>
	/// Hand an asset off to the Java metadata server in metadata_server/, which queues it up for the
	/// description to be written by hand. Never throws - the porting run shouldn't die because the
	/// server isn't running.
	/// </summary>
	public static async Task Send( AssetMeta asset )
	{
		string json = JsonSerializer.Serialize( asset );
		var content = new StringContent( json, Encoding.UTF8, "application/json" );

		try
		{
			var response = await HttpClient.PostAsync( "http://localhost:8080/submit", content );
			if ( !response.IsSuccessStatusCode )
				Log.Error( "Metadata server returned response code: " + response.StatusCode );
		}
		catch ( Exception e )
		{
			Log.Warning( $"Couldn't reach the metadata server for '{asset.PolyID}' - is it running? ({e.Message})" );
		}
	}
}
