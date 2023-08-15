using Editor;
using PolyHaven.API;
using PolyHaven.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PolyHaven.Meta;

public struct AssetMeta
{
	public AssetEntry AssetEntry { get; set; }

	[JsonPropertyName("poly_id")]
	public string PolyID { get; set; }
	[JsonPropertyName("asset_party_url")]
	public string? AssetPartyURL { get; set; }

	public string? Tags { get; set; }

	public AssetMeta(IPolyAsset asset)
	{
		AssetEntry = asset.Asset;
		PolyID = asset.PolyHavenID;
		AssetPartyURL = asset.SBoxAsset?.Package.Url;
		Tags = asset.SBoxAsset?.Publishing?.ProjectConfig.Tags;
	}

	public AssetMeta(Asset asset)
	{
		AssetEntry = asset.MetaData.Get<AssetEntry>( "PolyAsset" );
		PolyID = asset.MetaData.GetString( "polyhaven_id" );
		if (AssetEntry == null || PolyID == null)
		{
			throw new ArgumentException( "Asset does not have PolyHaven metadata.", nameof( asset ) );
		}

		AssetPartyURL = asset.Package?.Url;
		Tags = asset.Publishing?.ProjectConfig.Tags;
	}
}
