using PolyHaven.API;
using PolyHaven.Assets;
using System.Text.Json.Serialization;
namespace PolyHaven.Meta;

/// <summary>
/// What we hand the Java metadata server so the descriptions can be filled in by hand afterwards.
/// The property names have to keep matching the @SerializedName annotations on
/// metadata_server's AssetMeta/AssetEntry records.
/// </summary>
public struct AssetMeta
{
	public PolyHavenAssetInfo AssetEntry { get; set; }

	[JsonPropertyName( "poly_id" )]
	public string PolyID { get; set; }
	[JsonPropertyName( "asset_party_url" )]
	public string? AssetPartyURL { get; set; }

	public string? Tags { get; set; }

	public AssetMeta( IPolyAsset asset )
	{
		AssetEntry = asset.Info;
		PolyID = asset.PolyHavenId;

		// Null until the asset has actually been published - a dry run never gets a package back.
		AssetPartyURL = asset.SBoxAsset?.Package?.Url;

		// ProjectConfig has no Tags any more, so SetupMetadata stashes them on the asset instead.
		var tags = asset.SBoxAsset?.MetaData.Get<string[]>( "polyhaven_tags" );
		Tags = tags == null ? null : string.Join( ' ', tags );
	}
}
