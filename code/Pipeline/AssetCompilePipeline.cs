using PolyHaven.API;
using PolyHaven.AssetParty;
using PolyHaven.Meta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Pipeline;

public static class AssetCompilePipeline
{

	public static async Task DoCompile( string id, AssetEntry? entry = null )
	{
		if ( entry == null )
		{
			entry = await PolyHavenAPI.Instance.GetAsset( id );
			if ( entry.Type != API.AssetType.HDRI )
				throw new ArgumentException( "The supplied asset must be an HDRI.", nameof( id ) );
		}

		var asset = new PolyAsset( id, entry );
		await asset.DownloadHDR();

		var thumbTask = ThumbnailGenerator.GenerateThumbnail( asset );
		asset.GenerateMaterial();
		var thumb = await thumbTask;
		if ( thumb != null && asset.Material != null )
			ThumbnailGenerator.AssignThumbnail( asset.Material, thumb );

		asset.SetupMetadata();
		await AssetPublishing.Publish( asset );
		MetaServer.Send( new AssetMeta( asset ) );
		Log.Info( "Skybox generation complete!" );

	}

	/// <summary>
	/// Query Poly Haven and Asset Party and return all the hdris that haven't been ported yet.
	/// </summary>
	/// <returns>A map of asset ids and their relevent entries.</returns>
	public static async Task<IDictionary<string, AssetEntry>> GetUnfinishedAssets()
	{
		var assets = await PolyHavenAPI.Instance.GetAssets("hdris");
		var uploadedAssets = await AssetPartyProxy.ExistingPackages();

		var filteredAssets = new Dictionary<string, AssetEntry>();
		foreach ( var kv in assets )
		{
			if ( !(await AssetPartyProxy.PackageExists( kv.Key, uploadedAssets )) )
				filteredAssets.Add( kv.Key, kv.Value );
		}


		return filteredAssets;
	}

	private static bool ShouldStop = false;

	public static async Task DoMassCompile()
	{
		ShouldStop = false;
		Log.Info( "Starting mass compile." );
		Log.Info( "Searching for unported assets..." );

		var assets = await GetUnfinishedAssets();
		Log.Info( $"Found {assets.Count} assets" );

		foreach ( var asset in assets )
		{
			if ( ShouldStop ) return;
			Log.Info( $"Begining port for {asset.Key}" );
			await DoCompile( asset.Key, asset.Value );
		}
	}

	public static void StopCompile()
	{
		ShouldStop = true;
	}
}
