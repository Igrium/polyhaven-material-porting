using PolyHaven.API;
using PolyHaven.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Pipeline;

public class MaterialCompilePipeline
{
	public PolyHavenAPI API { get; set; } = PolyHavenAPI.Instance;

	public async Task SetupAsset( string id, AssetEntry? entry = null )
	{
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
		await asset.DownloadFiles();

		asset.GenerateSBoxAsset();
		asset.SetupMetadata();
	}
}
