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

	public AssetMeta(HDRIAsset asset)
	{
		AssetEntry = asset.Asset;
		PolyID = asset.PolyHavenID;
		AssetPartyURL = asset.AssetPartyURL;
		Tags = asset.SBoxAsset?.Publishing?.ProjectConfig.Tags;
	}
}
