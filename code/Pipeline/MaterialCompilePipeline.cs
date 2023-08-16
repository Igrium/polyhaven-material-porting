using PolyHaven.API;
using PolyHaven.Assets;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Pipeline;

public class MaterialCompilePipeline
{
	public PolyHavenAPI API { get; set; } = PolyHavenAPI.Instance;

	public async Task SetupAsset( string id, string resolution = "2k", string aoResolution = "1k", bool texOnly = false, AssetEntry? entry = null )
	{
		Log.Info( $"Downloading up {id} with a resolution of {resolution}" );
		Log.Info( "Retrieving asset metadata" );
		if ( entry == null )
		{
			entry = await API.GetAsset( id );
		}
		if ( entry == null || entry.Type != AssetType.Texture )
		{
			throw new ArgumentException( "The supplied asset must be a material.", nameof( id ) );
		}

		Log.Info( "Downloading textures..." );
		TextureMaterialAsset asset = new TextureMaterialAsset( id, entry );
		await asset.DownloadFiles(resolution, aoResolution);

		if (!texOnly)
		{
			asset.GenerateSBoxAsset();
			asset.SetupMetadata();
		}
	}


	[ConCmd.Engine( "download_material" )]
	public async static void DownloadMaterial( params string[] args )
	{
		string id = args[0];
		string res = "2k";

		var index = Array.IndexOf( args, "--res" );
		if ( index >= 0 )
		{
			res = args[index + 1];
		}

		string aoRes = res;
		index = Array.IndexOf( args, "--aoRes" );
		if (index >= 0)
		{
			aoRes = args[index + 1];
		}

		bool texOnly = args.Contains( "texOnly" );

		await new MaterialCompilePipeline().SetupAsset( id, res, aoRes, texOnly: texOnly );
		Log.Info( "Finished setting up material" );
	}

	private static IEnumerable<string> ReplaceSpaces( IEnumerable<string> src )
	{
		foreach ( string s in src )
		{
			yield return s.Replace( " ", "" );
		}
	}
}
